using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlTemplater;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Unit
{
    public class TestUser
    {
        public string Name { get; set; }
        public string Group { get; set; }

        public TestUser(string name, string group)
        {
            Name = name;
            Group = group;
        }
    }

    public class TestModel
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public bool IsAdmin { get; set; }
        public List<TestUser> Users { get; set; }
    }

    [TestClass]
    public class HtmlTemplateRendererTests
    {
        [TestMethod]
        public void RenderFromString_ValidProperty_ReplacesPlaceholder()
        {
            //Arrange
            TestUser user1 = new TestUser("Айрат", "11-541");
            string template = "Имя студента: ${user1.Name}";
            HtmlTemplateRenderer templateRenderer = new HtmlTemplateRenderer();

            //Act
            string result = templateRenderer.RenderFromString(template, user1);

            //Assert
            Assert.AreEqual("Имя студента: Айрат", result);
        }

        [TestMethod]
        public void RenderFromString_MultipleProperties_ReplacesAllPlaceholders()
        {
            //Arrange
            string template = "Пользователь ${user.Name} имеет роль ${user.Role}.";
            var model = new TestModel { Name = "Айрат", Role = "Разработчик" };
            var renderer = new HtmlTemplateRenderer();

            //Act
            string result = renderer.RenderFromString(template, model);

            //Assert
            Assert.AreEqual("Пользователь Айрат имеет роль Разработчик.", result);
        }

        [TestMethod]
        public void RenderFromString_EmptyTemplate_ReturnsEmptyString()
        {
            //Arrange
            string template = "";
            var model = new TestModel { Name = "Айрат" };
            var renderer = new HtmlTemplateRenderer();

            //Act
            string result = renderer.RenderFromString(template, model);

            //Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void RenderFromString_TrueCondition_RendersIfBlock()
        {
            //Arrange
            string template = "Доступ: $if(user.IsAdmin)Разрешен$elseЗапрещен$endif";
            var model = new TestModel { IsAdmin = true };

            //Act
            string result = TemplateParser.ProcessIfConditions(template, model);

            //Assert
            Assert.AreEqual("Доступ: Разрешен", result);
        }

        [TestMethod]
        public void RenderFromString_Collection_RendersAllItems()
        {
            //Arrange
            var listUser = new List<TestUser>
            {
                new TestUser("Айрат", "11-601"),
                new TestUser("Ильдар", "11-602")
            };
            string template = "$foreach(Users)Пользователь: $item.Name, Группа: $item.Group $endforeach";

            //Act
            string result = TemplateParser.ProcessForeach(template, listUser);

            //Assert
            string expected = "Пользователь: Айрат, Группа: 11-601 \nПользователь: Ильдар, Группа: 11-602 \n";
            Assert.AreEqual(expected, result);
        }
    }
}
