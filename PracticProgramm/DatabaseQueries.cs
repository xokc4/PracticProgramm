using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace PracticProgramm
{
    internal class DatabaseQueries
    {
        //  Обновляет имя компонента в таблице EngineComponents, заменяя старое имя на новое
        public static string UPDATEEngineComponents = "UPDATE EngineComponents " +
            "SET ComponentName = @NewName WHERE ComponentName = @CurrentName";
        //Обновляет имя вложенного элемента в таблице NestedElements, заменяя старое имя на новое.
        public static string UPDATENestedElements = "UPDATE NestedElements " +
            "SET ElementName = @NewName WHERE ElementName = @CurrentName";
        //Выбирает количество компонентов из таблицы EngineComponents, где имя компонента соответствует новому имени
        public static string SELECTEngineComponents= "SELECT COUNT(*) FROM EngineComponents " +
            "WHERE ComponentName = @NewName";
        //Выбирает количество компонентов из таблицы EngineComponents, где имя компонента соответствует новому имени.
        public static string SELECTNestedElements= "SELECT COUNT(*) FROM NestedElements " +
            "WHERE ElementName = @NewName";
        //Выбирает количество вложенных элементов из таблицы NestedElements, где имя вложенного элемента соответствует новому имени.
        public static string SELECTqueryComponents= @"
                SELECT ec.ComponentName, ec.ComponentCount
                FROM EngineComponents ec
                INNER JOIN AutoTable at ON ec.EngineID = at.EngineID
                WHERE at.CarName = @CarName";
        // Выбирает компоненты и их количество из таблицы EngineComponents, присоединенной к таблице AutoTable, где имя автомобиля соответствует заданному имени
        public static string SELECTqueryEngine = @"
            SELECT c.ComponentName, c.ComponentCount, e.ElementName, e.ElementCount
            FROM EngineComponents c
            LEFT JOIN NestedElements e ON c.ComponentID = e.ComponentID
            WHERE c.ComponentName = @ComponentName";
        // Выбирает компоненты, их количество, имена вложенных элементов и их количество из таблиц EngineComponents и NestedElements, где имя компонента соответствует заданному имени.
        public static string EngineComponentsEngineComponents= @"
                SELECT COUNT(*)
                FROM EngineComponents
                WHERE ComponentName = @ComponentName";
        //Выбирает количество компонентов из таблицы EngineComponents, где имя компонента соответствует заданному имени.
        public static string checkQuerySELECTNestedElements= @"
                    SELECT COUNT(*)
                    FROM NestedElements NE
                    INNER JOIN EngineComponents EC ON NE.ComponentID = EC.ComponentID
                    WHERE EC.ComponentName = @ComponentName
                    AND EC.IsNested = 1
                    AND EXISTS (
                        SELECT 1
                        FROM EngineComponents EC2
                        WHERE EC2.ComponentID = NE.ComponentID
                        AND EC2.IsNested = 1
                        AND EC2.ComponentName <> @ComponentName
                    )";
        //Проверяет, существует ли вложенный элемент в таблице NestedElements, связанный с компонентом в таблице EngineComponents, где имя компонента соответствует заданному имени и компонент является вложенным.
        public static string deleteNestedElementsQuery= @"
                    DELETE FROM NestedElements
                    WHERE ComponentID IN (
                        SELECT ComponentID
                        FROM EngineComponents
                        WHERE ComponentName = @ComponentName
                    )";
        // Удаляет вложенные элементы из таблицы NestedElements, связанные с компонентом в таблице EngineComponents, где имя компонента соответствует заданному имени.
        public static string deleteEngineComponentsQuery= @"
                    DELETE FROM EngineComponents
                    WHERE ComponentName = @ComponentName";
        // Удаляет компоненты из таблицы EngineComponents, где имя компонента соответствует заданному имени.
        public static string SELECTComponentNameFrom = "SELECT ComponentName FROM EngineComponents " +
            "WHERE ComponentName = @ComponentName";
        // Выбирает имя компонента из таблицы EngineComponents, где имя компонента соответствует заданному имени.
        public static string checkQuerySELECTComponentID = "SELECT ComponentID FROM EngineComponents " +
            "WHERE ComponentName = @ComponentName AND EngineID = @EngineID";
        //Выбирает идентификатор компонента из таблицы EngineComponents, где имя компонента и идентификатор двигателя соответствуют заданным значениям.
        public static string addComponentQuery= @"
                    INSERT INTO EngineComponents (ComponentName, EngineID, ComponentCount, IsNested)
                    OUTPUT INSERTED.ComponentID
                    VALUES (@ComponentName, @EngineID, @ComponentCount, @IsNested)";
        //Добавляет новый компонент в таблицу EngineComponents и возвращает идентификатор добавленного компонента.
        public static string addNestedElementsQuery= @"
                    INSERT INTO NestedElements (ComponentID, ElementName, ElementCount)
                    VALUES (@ComponentID, @ElementName, @ElementCount)";
        // Выбирает идентификатор двигателя из таблицы AutoTable, где имя автомобиля соответствует заданному имени.
        public static string checkCarSql= "SELECT EngineID FROM AutoTable WHERE CarName = @CarName";
        //Выбирает количество компонентов из таблицы EngineComponents, где имя компонента соответствует новому имени.
        public static string checkEngineComponentsQuery= "SELECT COUNT(*) FROM EngineComponents " +
            "WHERE ComponentName = @NewName";
        //Выбирает количество вложенных элементов из таблицы NestedElements, где имя вложенного элемента соответствует новому имени.
        public static string checkNestedElementsQuery= "SELECT COUNT(*) FROM NestedElements WHERE ElementName = @NewName";
        /// <summary>
        /// метод проверяет существует ли таблицы в бд
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static bool CheckTableExists(string tableName,string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
                    command.Parameters.AddWithValue("@TableName", tableName);

                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        /// <summary>
        /// создание бд с помощью T-sql
        /// </summary>
        /// <param name="connectionString"></param>
        public static void CreateTables(string connectionString)
        {
            string[] tableScripts = new string[]
            {
            @"CREATE TABLE AutoTable (
            CarID INT IDENTITY(1,1) NOT NULL, 
            CarName NVARCHAR(100) NULL,
            EngineID INT NULL,
            CONSTRAINT PK_AutoTable PRIMARY KEY CLUSTERED (CarID ASC)
                );",
            @"CREATE TABLE EngineComponents (
            ComponentID INT IDENTITY(1,1) NOT NULL, 
            EngineID INT NULL, 
            ComponentName VARCHAR(255) NULL, 
            ComponentCount INT NULL, 
            IsNested BIT NULL, 
            CONSTRAINT PK_EngineComponents PRIMARY KEY CLUSTERED (ComponentID ASC)
                );",
            @"CREATE TABLE NestedElements (
            ElementID INT IDENTITY(1,1) NOT NULL, 
            ComponentID INT NULL, 
            ElementName VARCHAR(255) NULL, 
            ElementCount INT NULL, 
            CONSTRAINT PK_NestedElements PRIMARY KEY CLUSTERED (ElementID ASC)
                );",
            @"INSERT INTO AutoTable (CarName, EngineID)
            VALUES
            ('Веста', 1),
            ('BWM', 2);",
            @"INSERT INTO EngineComponents (EngineID, ComponentName, ComponentCount, IsNested)
            VALUES
            (1, 'Блок цилиндров', 1, 0),
            (1, 'Коленвал', 1, 0),
            (1, 'Распредвал', 2, 0),
            (1, 'Поршень в сборе', 4, 1),
            (1, 'Шатун', 4, 0);",
            @"INSERT INTO NestedElements (ComponentID, ElementName, ElementCount)
            VALUES
            (4, 'Поршень', 1),
            (4, 'Компрессионное кольцо', 2),
            (4, 'Маслосъемное кольцо', 1);"
            };
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (string script in tableScripts)
                {
                    using (SqlCommand command = new SqlCommand(script, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
