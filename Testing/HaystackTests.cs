using System.Collections.Generic;
using System.Linq;
using IMS.Common.Needle.Extensions;
using IMS.Tests.Needle.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IMS.Tests.Needle
{
    [TestClass]
    public class HaystackTests
    {
        [TestMethod]
        public void TestAutoHaystackShouldMatch()
        {
            var i = new AutoHaystackTestModel
            {
                ExampleInt = 25,
                ExampleString = "this is a string",
                Hidden = "secret",
                ShowHidden = false
            };

            var ah = Common.Needle.Needle.HaystackFactory(i);
            Assert.AreEqual($"this is a string{NeedleSearchExtensions.Delimiter}25", ah);
        }

        [TestMethod]
        public void TestAutoHaystackWithNestedShouldMatch()
        {
            var i = new NestedAutoHaystackTestModel
            {
                TestModel = new AutoHaystackTestModel
                {
                    Hidden = "secret"
                },
                Deep = new NestedAutoHaystackTestModel
                {
                    TestModel = new AutoHaystackTestModel
                    {
                        Hidden = "deep secret"
                    }
                }
            };

            var ah = Common.Needle.Needle.HaystackFactory(i);
            Assert.AreEqual($"secret{NeedleSearchExtensions.Delimiter}deep secret", ah);
        }

        [TestMethod]
        public void TestAutoHaystackWithNullNestedShouldMatch()
        {
            var i = new NestedAutoHaystackTestModel
            {
                TestModel = null,
                Deep = null
            };

            var ah = Common.Needle.Needle.HaystackFactory(i);
            Assert.AreEqual("", ah);
            Assert.AreEqual(null, i.TestModel);
            Assert.AreEqual(null, i.Deep);
        }

        [TestMethod]
        public void TestAutoHaystackIsVisibleShouldMatch()
        {
            var i = new AutoHaystackTestModel
            {
                ExampleInt = 25,
                ExampleString = "this is a string",
                Hidden = "secret",
                ShowHidden = true
            };

            var ah = Common.Needle.Needle.HaystackFactory(i);
            Assert.AreEqual(
                $"this is a string{NeedleSearchExtensions.Delimiter}25{NeedleSearchExtensions.Delimiter}secret", ah);
        }

        [TestMethod]
        public void TestAutoHaystackIsVisible2ShouldMatch()
        {
            var i = new AutoHaystackTestModel
            {
                ExampleInt = 25,
                ExampleString = "this is a string",
                MinLength = 1000
            };

            var ah = Common.Needle.Needle.HaystackFactory(i);
            Assert.AreEqual("25", ah);
        }

        [TestMethod]
        public void TestHaystackUpdateShouldMatch()
        {
            const string g1 = "00000";
            const string g2 = "55555";

            var i = new AutoHaystackTestModel
            {
                ExampleString = g1
            };

            var h1 = Common.Needle.Needle.HaystackFactory(i);

            i.ExampleString = g2;
            i.ExampleInt = 1;
            var h2 = Common.Needle.Needle.HaystackFactory(i);

            Assert.AreEqual("00000ᐰ0", h1);
            Assert.AreEqual("55555ᐰ1", h2);
        }

        [TestMethod]
        public void TestStringListInHaystackShouldMatch()
        {
            var o = new NestedCollectionAutoHaystackTestModel
            {
                TestString = "asdf",
                Substrings = "q w e r t y".Split(' ').ToList()
            };

            var h = Common.Needle.Needle.HaystackFactory(o);

            Assert.AreEqual("asdfᐰqꓩwꓩeꓩrꓩtꓩy", h);
        }

        [TestMethod]
        public void TestModelListInHaystackShouldMatch()
        {
            var o = new NestedCollectionAutoHaystackTestModel
            {
                TestString = "asdf",
                Submodels = new List<NestedAutoHaystackTestModel>
                {
                    new NestedAutoHaystackTestModel
                    {
                        TestModel = new AutoHaystackTestModel
                        {
                            Hidden = "is"
                        }
                    },
                    new NestedAutoHaystackTestModel
                    {
                        TestModel = new AutoHaystackTestModel
                        {
                            Hidden = "this"
                        }
                    },
                    new NestedAutoHaystackTestModel
                    {
                        TestModel = new AutoHaystackTestModel
                        {
                            Hidden = "real"
                        }
                    },
                    new NestedAutoHaystackTestModel
                    {
                        TestModel = new AutoHaystackTestModel
                        {
                            Hidden = "life?"
                        }
                    }
                }
            };

            var h = Common.Needle.Needle.HaystackFactory(o);

            Assert.AreEqual("asdfᐰᐰisꗏthisꗏrealꗏlife?", h);
        }

        [TestMethod]
        public void TestMetadataAnnotationsShouldMatch()
        {
            var o = new MetadataAutoHaystackTestModel
            {
                First = "qwerty",
                Second = "asdfg",
                Third = "zxcvb",
                Fourth = "12345"
            };

            var h = Common.Needle.Needle.HaystackFactory(o);

            Assert.AreEqual("qwertyᐰzxcvb", h);
        }

        [TestMethod]
        public void TestMixedAnnotationsShouldMatch()
        {
            var o = new MixedAutoHaystackTestModel
            {
                First = "poiuy",
                Second = "lkjhg",
                Third = "mnbvc",
                Fourth = "09876"
            };

            var h = Common.Needle.Needle.HaystackFactory(o);

            Assert.AreEqual("09876ᐰmnbvc", h);
        }
    }
}