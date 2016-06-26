using System;
using DediLib.Data;
using NUnit.Framework;

namespace DediLib.Tests.Data
{
    [TestFixture]
    public class TestSqlBuilder
    {
        [Test]
        public void Select_NoColumns_SelectAllFromTable()
        {
            var sqlBuilder = new SqlBuilder();
            Assert.AreEqual("SELECT * FROM table", sqlBuilder.Select("table"));
        }

        [Test]
        public void Select_SingleColumn_SelectSingleColumnFromTable()
        {
            var sqlBuilder = new SqlBuilder();
            Assert.AreEqual("SELECT column FROM table", sqlBuilder.Select("table", "column"));
        }

        [Test]
        public void Select_MultipleColumns_SelectMultipleColumnsFromTable()
        {
            var sqlBuilder = new SqlBuilder();
            Assert.AreEqual("SELECT column1,column2 FROM table", sqlBuilder.Select("table", "column1", "column2"));
        }

        [Test]
        public void Update_NoColumns_Throws()
        {
            var sqlBuilder = new SqlBuilder();
            Assert.Throws<ArgumentException>(() => sqlBuilder.Update("table"));
        }

        [Test]
        public void Update_SingleColumn_SelectSingleColumnFromTable()
        {
            var sqlBuilder = new SqlBuilder();
            Assert.AreEqual("UPDATE table SET column=@column", sqlBuilder.Update("table", "column"));
        }

        [Test]
        public void Update_MultipleColumns_SelectMultipleColumnsFromTable()
        {
            var sqlBuilder = new SqlBuilder();
            Assert.AreEqual("UPDATE table SET column1=@column1,column2=@column2", sqlBuilder.Update("table", "column1", "column2"));
        }
    }
}
