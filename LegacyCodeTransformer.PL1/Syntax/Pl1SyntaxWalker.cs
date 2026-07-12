using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Syntax
{
    /// <summary>
    /// PL/I syntax tree üzerinde varsayılan recursive traversal davranışı
    /// sağlayan walker sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// Visitor sınıfı syntax node türleri için merkezi dispatch davranışı
    /// sağlar; ancak child node'ların recursive olarak dolaşılması her
    /// visitor implementasyonunun ortak ihtiyacı değildir.
    ///
    /// Walker, syntax tree içindeki declaration, procedure, statement,
    /// expression, data type ve initial value modellerinin varsayılan
    /// recursive traversal davranışını merkezi hale getirir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Syntax tree, global declaration, procedure declaration, procedure
    /// statement, structure member, data type, initial value, statement ve
    /// expression modellerini tutarlı ve override edilebilir şekilde
    /// dolaşır.
    ///
    /// Procedure body içinde bulunan declaration ve executable statement
    /// koleksiyonlarının traversal sırasında kaybolmasını engeller.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL ERROR_TEXT CHAR(50);
    ///
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///
    ///     ERROR_TEXT = PROCESS_TEXT;
    ///     CALL WRITE_ERROR(ERROR_TEXT);
    /// END CUSTOMER_PROCESS;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1ReferenceCollector
    /// - Semantic analyzer traversal işlemleri
    /// - Procedure declaration analizleri
    /// - Metrics ve dependency analyzer işlemleri
    /// - Visitor tabanlı gelecekteki transpiler geliştirmeleri
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Procedure local symbol table, parameter declaration binding,
    /// structure member resolution, SELECT, READ, WRITE, RETURN, STOP,
    /// LEAVE ve diğer PL/I modellerinin recursive traversal davranışına
    /// temel olur.
    /// </summary>
    public class Pl1SyntaxWalker : Pl1SyntaxVisitor
    {
        /// <summary>
        /// Root PL/I syntax tree içindeki bütün ana koleksiyonları dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// Global declaration, procedure ve top-level statement modellerinin
        /// tamamı aynı traversal başlangıç noktasından ziyaret edilmelidir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Syntax tree üzerindeki declaration, procedure ve statement
        /// koleksiyonlarını sırasıyla visitor dispatch altyapısına gönderir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// Global DCL alanları, top-level compiler directive satırları ve
        /// procedure modellerini aynı syntax tree içinde destekler.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Visit(Pl1SyntaxTree) çağrısı sonrasında otomatik olarak çalışır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Syntax tree'ye yeni root koleksiyonları eklenirse traversal
        /// davranışı bu method üzerinden genişletilir.
        /// </summary>
        protected override void VisitSyntaxTree(Pl1SyntaxTree syntaxTree)
        {
            foreach (var declaration in syntaxTree.Declarations)
            {
                Visit(declaration);
            }

            foreach (var procedure in syntaxTree.Procedures)
            {
                Visit(procedure);
            }

            foreach (var statement in syntaxTree.Statements)
            {
                Visit(statement);
            }
        }

        /// <summary>
        /// Procedure içindeki declaration ve executable statement
        /// modellerini dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// Gerçek PL/I procedure'lerinde parameter veya local variable
        /// declaration bilgileri procedure body içinde bulunabilir.
        ///
        /// Walker yalnızca executable statement listesini dolaşırsa
        /// procedure declaration modelleri semantic analyzer ve diğer
        /// visitor tabanlı işlemler tarafından görülemez.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Procedure declaration koleksiyonunu ve executable statement
        /// koleksiyonunu mevcut visitor dispatch altyapısıyla dolaşır.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
        ///     DCL PROCESS_TEXT CHAR(50);
        ///     ERROR_TEXT = PROCESS_TEXT;
        /// END CUSTOMER_PROCESS;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Procedure syntax node'u walker tarafından ziyaret edildiğinde
        /// otomatik olarak çalışır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Procedure local symbol collection, parameter binding, scope
        /// analysis ve EGL function parameter çözümlemesine temel olur.
        /// </summary>
        protected override void VisitProcedure(Pl1Procedure procedure)
        {
            foreach (var declaration in procedure.Declarations)
            {
                Visit(declaration);
            }

            foreach (var statement in procedure.Statements)
            {
                Visit(statement);
            }
        }

        /// <summary>
        /// Variable declaration içindeki data type ve initial value
        /// modellerini dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// Declaration yalnızca isim bilgisinden oluşmaz; data type ve
        /// initial value modelleri de syntax tree'nin child node'larıdır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Variable declaration analizlerinde data type ve initial value
        /// bilgilerinin visitor tabanlı işlemler tarafından görülmesini
        /// sağlar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// DCL PROCESS_TEXT CHAR(50) INIT(' ');
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Global veya procedure içindeki variable declaration traversal
        /// işlemlerinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Type resolution ve initial value semantic analizine temel olur.
        /// </summary>
        protected override void VisitVariableDeclaration(
            Pl1VariableDeclaration declaration)
        {
            Visit(declaration.DataType);

            if (declaration.InitialValue is not null)
            {
                Visit(declaration.InitialValue);
            }
        }

        /// <summary>
        /// Structure declaration içindeki member modellerini dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I structure declaration modelleri nested member hiyerarşisi
        /// içerir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Structure'ın doğrudan child member modellerini visitor dispatch
        /// altyapısına gönderir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// DCL 1 CUSTOMER_INFO,
        ///     5 CUSTOMER_NO CHAR(8),
        ///     5 CUSTOMER_NAME CHAR(30);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Global veya procedure içindeki structure traversal işlemlerinde
        /// kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Structure member symbol resolution ve type analysis
        /// davranışlarına temel olur.
        /// </summary>
        protected override void VisitStructureDeclaration(
            Pl1StructureDeclaration declaration)
        {
            foreach (var member in declaration.Members)
            {
                Visit(member);
            }
        }

        /// <summary>
        /// Structure member içindeki data type, initial value ve child member
        /// modellerini dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// Structure member modelleri hem field hem de nested group olarak
        /// kullanılabilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Member data type, initial value ve nested member hiyerarşisinin
        /// recursive olarak ziyaret edilmesini sağlar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// DCL 1 CUSTOMER_INFO,
        ///     5 ADDRESS_INFO,
        ///         10 CITY_CODE CHAR(3);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Structure traversal sırasında kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Qualified identifier ve structure field semantic çözümlemesine
        /// temel olur.
        /// </summary>
        protected override void VisitStructureMember(
            Pl1StructureMember member)
        {
            if (member.DataType is not null)
            {
                Visit(member.DataType);
            }

            if (member.InitialValue is not null)
            {
                Visit(member.InitialValue);
            }

            foreach (var childMember in member.Members)
            {
                Visit(childMember);
            }
        }

        /// <summary>
        /// Assignment statement hedef ve değer expression modellerini
        /// dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// Assignment statement semantic reference bilgisi hem hedef hem de
        /// değer tarafında bulunabilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Bütün assignment target expression modellerini ve value
        /// expression modelini visitor dispatch altyapısına gönderir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// CUSTOMER_NO = MUST_NO;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Pl1ReferenceCollector ve gelecekteki type analysis işlemlerinde
        /// kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Assignment type compatibility ve write-reference analizine temel
        /// olur.
        /// </summary>
        protected override void VisitAssignmentStatement(
            Pl1AssignmentStatement statement)
        {
            foreach (var target in statement.Targets)
            {
                Visit(target);
            }

            Visit(statement.Value);
        }

        /// <summary>
        /// PL/I CALL statement argument expression modellerini dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// Procedure veya function invocation argument'ları symbol reference
        /// ve type bilgisi taşıyabilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// CALL argument listesindeki bütün expression modellerini visitor
        /// dispatch altyapısına gönderir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// CALL WRITE_ERROR(ERROR_TEXT);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Reference collector ve ileride procedure call binding
        /// analizlerinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Procedure parameter eşleştirmesi ve argument type analizine temel
        /// olur.
        /// </summary>
        protected override void VisitCallStatement(
            Pl1CallStatement statement)
        {
            foreach (var argument in statement.Arguments)
            {
                Visit(argument);
            }
        }

        /// <summary>
        /// IF condition ve branch statement modellerini dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// IF statement condition, THEN ve opsiyonel ELSE child modelleri
        /// içerir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Condition expression ile THEN ve ELSE branch modellerini
        /// recursive traversal akışına dahil eder.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// IF CUSTOMER_NO = MUST_NO THEN
        ///     CALL FETCH_CUSTOMER;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Control-flow traversal işlemlerinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Condition semantic analysis ve control-flow graph çalışmalarına
        /// temel olur.
        /// </summary>
        protected override void VisitIfStatement(
            Pl1IfStatement statement)
        {
            Visit(statement.Condition);
            Visit(statement.ThenStatement);

            if (statement.ElseStatement is not null)
            {
                Visit(statement.ElseStatement);
            }
        }

        /// <summary>
        /// DO statement condition ve body modellerini dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I DO statement block, WHILE veya UNTIL biçimlerinde condition
        /// ve body bilgisi taşıyabilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Opsiyonel condition expression modelini ve body block modelini
        /// recursive olarak ziyaret eder.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// DO WHILE(SQLCODE = 0);
        ///     CALL FETCH_CURSOR;
        /// END;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Nested statement traversal işlemlerinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Loop semantic analysis ve control-flow graph çalışmalarına temel
        /// olur.
        /// </summary>
        protected override void VisitDoStatement(
            Pl1DoStatement statement)
        {
            if (statement.Condition is not null)
            {
                Visit(statement.Condition);
            }

            Visit(statement.Body);
        }

        /// <summary>
        /// Block statement içindeki child statement modellerini dolaşır.
        ///
        /// Neden var?
        /// ----------------------
        /// IF THEN DO ve standalone DO yapıları birden fazla child statement
        /// içerebilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Block içindeki statement modellerinin kaynak sırasıyla ziyaret
        /// edilmesini sağlar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// DO;
        ///     CUSTOMER_NO = MUST_NO;
        ///     CALL FETCH_CUSTOMER;
        /// END;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Nested block traversal işlemlerinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Scope ve control-flow analizlerine temel olur.
        /// </summary>
        protected override void VisitBlockStatement(
            Pl1BlockStatement statement)
        {
            foreach (var childStatement in statement.Statements)
            {
                Visit(childStatement);
            }
        }

        /// <summary>
        /// Embedded SQL statement için varsayılan leaf-node traversal
        /// davranışını tanımlar.
        ///
        /// Neden var?
        /// ----------------------
        /// Embedded SQL modeli şu anda raw SQL text taşır ve ziyaret edilecek
        /// güçlü tipli child syntax node içermez.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Embedded SQL statement'ın visitor dispatch zincirinde geçerli bir
        /// leaf node olarak kalmasını sağlar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// EXEC SQL INCLUDE SQLCA;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Syntax walker traversal işlemlerinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// SQL host variable modeli eklendiğinde traversal davranışı burada
        /// genişletilebilir.
        /// </summary>
        protected override void VisitEmbeddedSqlStatement(
            Pl1EmbeddedSqlStatement statement)
        {
        }

        /// <summary>
        /// Compiler directive statement için varsayılan leaf-node traversal
        /// davranışını tanımlar.
        ///
        /// Neden var?
        /// ----------------------
        /// Compiler directive modeli şu anda string metadata taşır ve ziyaret
        /// edilecek güçlü tipli child syntax node içermez.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Compiler directive statement'ın visitor dispatch zincirinde
        /// geçerli bir leaf node olarak kalmasını sağlar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// %INCLUDE COPYLIB;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Syntax walker traversal işlemlerinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Directive argument modelleri syntax node haline gelirse traversal
        /// davranışı burada genişletilebilir.
        /// </summary>
        protected override void VisitCompilerDirectiveStatement(
            Pl1CompilerDirectiveStatement statement)
        {
        }

        /// <summary>
        /// Raw expression için varsayılan leaf-node traversal davranışını
        /// tanımlar.
        ///
        /// Neden var?
        /// ----------------------
        /// Pl1RawExpression henüz daha küçük expression node modellerine
        /// ayrılmamıştır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Pl1ReferenceCollector gibi derived walker sınıflarının raw
        /// expression ziyaretini override edebilmesini sağlar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// CUSTOMER_NO
        /// CUSTOMER_INFO.MUST_NO
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Pl1ReferenceCollector tarafından override edilir.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Full expression tree eklendiğinde expression traversal ailesinin
        /// genişletilmesine temel olur.
        /// </summary>
        protected override void VisitRawExpression(
            Pl1RawExpression expression)
        {
        }
    }
}