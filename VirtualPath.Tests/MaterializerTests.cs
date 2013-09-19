using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VirtualPath.Tests
{
    public interface IExportTest
    {

    }

    [Export("Test", typeof(IExportTest))]
    public class ExportTest : IExportTest
    {
        public string P1;
        public int I1;

        public ExportTest() { }
        public ExportTest(string p1) { this.P1 = p1; }
        public ExportTest(int i1) { this.I1 = i1; }
    }

    public class MaterializerTests
    {
        [Test]
        public void GetExportedType()
        {
            var materializer = Materializer.Default;
            var type = materializer.FindType<IExportTest>("Test");
            Assert.That(type, Is.EqualTo(typeof(ExportTest)));
        }

        [Test]
        public void Materialize_WithNoParameter()
        {
            var materializer = Materializer.Default;
            var config = new Dictionary<string,object>();
            //var type = materializer.GetComposedType<IExportTest>("Test");
            var instance = materializer.Materialize<IExportTest>("Test", config);
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<ExportTest>());
        }

        [Test]
        public void Materialize_WithOneStringParameter()
        {
            var materializer = Materializer.Default;
            var config = new Dictionary<string, object> 
            {
                { "p1", "value" },
            };
            var instance = materializer.Materialize<IExportTest>("Test", config);
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<ExportTest>());
            Assert.That(((ExportTest)instance).P1, Is.EqualTo(config["p1"]));
        }

        [Test]
        public void Materialize_WithOneIntParameterAsString()
        {
            var materializer = Materializer.Default;
            var config = new Dictionary<string, object> 
            {
                { "i1", "1" },
            };
            var instance = materializer.Materialize<IExportTest>("Test", config);
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<ExportTest>());
            Assert.That(((ExportTest)instance).I1, Is.EqualTo(1));
        }
    }
}
