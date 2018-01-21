using System.Collections.Generic;
using System.Linq;
using IMS.Common.Needle;
using IMS.Common.Needle.Extensions;
using IMS.Tests.Needle.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IMS.Tests.Needle
{
    [TestClass]
    public class HaystackSearchTests
    {
        private static IEnumerable<AutoHaystackTestModel> AutoTestData => new List<AutoHaystackTestModel>
        {
            new AutoHaystackTestModel
            {
                ExampleInt = 1,
                ExampleString = "aaaaaa"
            },
            new AutoHaystackTestModel
            {
                ExampleInt = 3,
                ExampleString = "a string"
            },
            new AutoHaystackTestModel
            {
                ExampleInt = 5,
                ExampleString = "yet another string"
            },
            new AutoHaystackTestModel
            {
                ExampleInt = 1,
                ExampleString = "eeeeeeeeee",
                ShowHidden = true,
                Hidden = "secret"
            },
            new AutoHaystackTestModel
            {
                ExampleInt = -5,
                ExampleString = "oooooooooooooooooo",
                ShowHidden = false,
                Hidden = "secret"
            },
            new AutoHaystackTestModel
            {
                ExampleInt = -50,
                ExampleString = "lll",
                MinLength = 4,
                Custom = "custom stuff",
                ShowCustom = true
            },
            new AutoHaystackTestModel
            {
                ExampleInt = -55,
                ExampleString = "999 999 888 777 *** &&&"
            },
            new AutoHaystackTestModel
            {
                ExampleInt = -5,
                ExampleString = "^^^ $$$ %%% @@@ 999 888"
            }
        };

        [TestMethod]
        public void TestAutoHaystackSearchStringShouldMatch()
        {
            var i = AutoTestData.AsQueryable();

            var q = i.Search("string");
            var f = q.OrderByDescending(x => x.ExampleInt).ToList();

            var o1 = f[0];
            var o2 = f[1];

            Assert.AreEqual(2, f.Count);
            Assert.AreEqual("yet another string", o1.ExampleString);
            Assert.AreEqual("a string", o2.ExampleString);
        }

        [TestMethod]
        public void TestAutoHaystackSearchMultipleStringsShouldMatch()
        {
            var i = AutoTestData.AsQueryable();

            var q = i.Search("@@@ ***");
            var f = q.OrderByDescending(x => x.ExampleInt).ToList();

            var o1 = f[0];
            var o2 = f[1];

            Assert.AreEqual(2, f.Count);
            Assert.AreEqual("^^^ $$$ %%% @@@ 999 888", o1.ExampleString);
            Assert.AreEqual("999 999 888 777 *** &&&", o2.ExampleString);
        }

        [TestMethod]
        public void TestAutoHaystackSearchIntShouldMatch()
        {
            var i = AutoTestData.AsQueryable();

            var q = i.Search("1");
            var f = q.OrderByDescending(x => x.ExampleString).ToList();

            var o1 = f[0];
            var o2 = f[1];

            Assert.AreEqual(2, f.Count);
            Assert.AreEqual("eeeeeeeeee", o1.ExampleString);
            Assert.AreEqual("aaaaaa", o2.ExampleString);
        }

        [TestMethod]
        public void TestAutoHaystackSearchRenderShouldMatch()
        {
            var i = AutoTestData.AsQueryable();

            var q = i.Search("¶");
            var f = q.OrderByDescending(x => x.ExampleString).ToList();

            var o = f[0];

            Assert.AreEqual(1, f.Count);
            Assert.AreEqual("custom stuff", o.Custom);
        }

        [TestMethod]
        public void TestAuthoHaystackSearchWithNestedshouldMatch()
        {
            var i = new List<NestedAutoHaystackTestModel>
            {
                new NestedAutoHaystackTestModel
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
                },
                new NestedAutoHaystackTestModel
                {
                    TestModel = new AutoHaystackTestModel
                    {
                        Hidden = "aaaaaaaaaa"
                    },
                    Deep = new NestedAutoHaystackTestModel
                    {
                        TestModel = new AutoHaystackTestModel
                        {
                            Hidden = "eeee"
                        }
                    }
                }
            }.AsQueryable();

            var q = i.Search("deep");
            var f = q.OrderByDescending(x => Common.Needle.Needle.HaystackFactory(x)).ToList();
            var s = f.Single();

            Assert.AreEqual("deep secret", s.Deep.TestModel.Hidden);
        }

        [TestMethod]
        public void TestDeepSearchCollectionShouldMatch()
        {
            var c = new List<NestedCollectionAutoHaystackTestModel>
            {
                new NestedCollectionAutoHaystackTestModel
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
                },
                new NestedCollectionAutoHaystackTestModel
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
                                Hidden = "a"
                            }
                        },
                        new NestedAutoHaystackTestModel
                        {
                            TestModel = new AutoHaystackTestModel
                            {
                                Hidden = "fantasy?"
                            }
                        }
                    }
                }
            };

            var i = c.AsQueryable();
            var q = i.Search("fantasy");

            var r = q.ToList().Single();

            Assert.AreEqual("asdfᐰᐰisꗏthisꗏaꗏfantasy?", Common.Needle.Needle.HaystackFactory(r));
        }

        [TestMethod]
        public void TestReallyDeepSearchCollectionShouldMatch()
        {
            var c = new List<NestedCollectionAutoHaystackTestModel>
            {
                new NestedCollectionAutoHaystackTestModel
                {
                    TestString = "asdf",
                    LinkedModels = new List<NestedCollectionAutoHaystackTestModel>
                    {
                        new NestedCollectionAutoHaystackTestModel
                        {
                            TestString = "asdf",
                            Submodels = new List<NestedAutoHaystackTestModel>
                            {
                                new NestedAutoHaystackTestModel
                                {
                                    TestModel = new AutoHaystackTestModel
                                    {
                                        Custom = "is"
                                    }
                                },
                                new NestedAutoHaystackTestModel
                                {
                                    TestModel = new AutoHaystackTestModel
                                    {
                                        Custom = "this"
                                    }
                                },
                                new NestedAutoHaystackTestModel
                                {
                                    TestModel = new AutoHaystackTestModel
                                    {
                                        Custom = "real"
                                    }
                                },
                                new NestedAutoHaystackTestModel
                                {
                                    TestModel = new AutoHaystackTestModel
                                    {
                                        Custom = "life?"
                                    }
                                }
                            }
                        }
                    }
                },
                new NestedCollectionAutoHaystackTestModel
                {
                    TestString = "asdf",
                    LinkedModels = new List<NestedCollectionAutoHaystackTestModel>
                    {
                        new NestedCollectionAutoHaystackTestModel
                        {
                            TestString = "asdf",
                            Submodels = new List<NestedAutoHaystackTestModel>
                            {
                                new NestedAutoHaystackTestModel
                                {
                                    TestModel = new AutoHaystackTestModel
                                    {
                                        Custom = "is"
                                    }
                                },
                                new NestedAutoHaystackTestModel
                                {
                                    TestModel = new AutoHaystackTestModel
                                    {
                                        Custom = "this"
                                    }
                                },
                                new NestedAutoHaystackTestModel
                                {
                                    TestModel = new AutoHaystackTestModel
                                    {
                                        Custom = "a"
                                    }
                                },
                                new NestedAutoHaystackTestModel
                                {
                                    TestModel = new AutoHaystackTestModel
                                    {
                                        Custom = "fantasy?"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var i = c.AsQueryable();
            var q = i.Search("fantasy");

            var r = q.ToList().Single();

            Assert.AreEqual("asdfᐰᐰᐰisꗏthisꗏaꗏfantasy?", Common.Needle.Needle.HaystackFactory(r));
        }


        [TestMethod]
        public void TestAutoHaystackMatchStringShouldMatch()
        {
            var i = AutoTestData.AsQueryable();

            var q = i.Search("string");
            var f = q.OrderByDescending(x => x.ExampleInt).ToList();
            var matches = new List<Match>();

            foreach (var testModel in f)
            {
                matches.AddRange(testModel.FindMatches("string"));
            }

            Assert.AreEqual(2, matches.Count);

            var m1 = matches[0];
            var m2 = matches[1];

            Assert.AreEqual("ExampleString", m1.Path);
            Assert.AreEqual("ExampleString", m2.Path);

            Assert.AreEqual("yet another string", m1.Value);
            Assert.AreEqual("a string", m2.Value);
        }

        [TestMethod]
        public void TestAutoHaystackMatchMultipleShouldMatch()
        {
            var i = AutoTestData.AsQueryable();

            var q = i.Search("1 ooooo");
            var f = q.OrderByDescending(x => x.ExampleString).ToList();
            var matches = new List<Match>();

            foreach (var testModel in f)
            {
                matches.AddRange(testModel.FindMatches("1 ooooo"));
            }

            Assert.AreEqual(3, matches.Count);

            var m1 = matches[0];
            var m2 = matches[1];
            var m3 = matches[2];

            Assert.AreEqual("ExampleString", m1.Path);
            Assert.AreEqual("ExampleInt", m2.Path);
            Assert.AreEqual("ExampleInt", m3.Path);

            Assert.AreEqual("oooooooooooooooooo", m1.Value);
            Assert.AreEqual("1", m2.Value);
            Assert.AreEqual("1", m3.Value);
        }
    }
}