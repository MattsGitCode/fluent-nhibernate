using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Mapping;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.MappingModel.ClassBased;
using NUnit.Framework;

namespace FluentNHibernate.Testing.PersistenceModelTests
{
    [TestFixture]
    public class SeparateSubclassVisitorFixture
    {
        private IList<IIndeterminateSubclassMappingProvider> providers;
        private ClassMapping fooMapping;

        [SetUp]
        public void SetUp()
        {
            providers = new List<IIndeterminateSubclassMappingProvider>();
        }

        [Test]
        public void Should_add_subclass_that_implements_the_parent_interface()
        {
            /* The Parent is the IFoo interface the desired results 
             * of this test is the inclusion of the Foo<T> through the
             * GenericFooMap<T> subclass mapping.
             */

            fooMapping = ((IMappingProvider)new FooMap()).GetClassMapping();

            providers.Add(new StringFooMap());
            var sut = CreateSut();
            sut.ProcessClass(fooMapping);
            Assert.AreEqual(1, fooMapping.Subclasses.Count());
            Assert.AreEqual(1, fooMapping.Subclasses.Where(sub => sub.Type.Equals(typeof(Foo<string>))).Count());
        }

        [Test]
        public void Should_add_subclass_that_implements_the_parent_base()
        {
            /* The Parent is the FooBase class the desired results 
             * of this test is the inclusion of the Foo<T> through the
             * GenericFooMap<T> subclass mapping.
             */

            fooMapping = ((IMappingProvider)new BaseMap()).GetClassMapping();

            providers.Add(new StringFooMap());
            var sut = CreateSut();
            sut.ProcessClass(fooMapping);
            Assert.AreEqual(1, fooMapping.Subclasses.Count());
            Assert.AreEqual(1, fooMapping.Subclasses.Where(sub => sub.Type.Equals(typeof(Foo<string>))).Count());
        }

        [Test]
        public void Should_not_add_subclassmap_that_does_not_implement_parent_interface()
        {
            /* The Parent is the IFoo interface the desired results 
             * of this test is the exclusion of the StandAlone class 
             * since it does not implement the interface.
             */

            fooMapping = ((IMappingProvider)new FooMap()).GetClassMapping();

            providers.Add(new StandAloneMap());
            var sut = CreateSut();
            sut.ProcessClass(fooMapping);
            Assert.AreEqual(0, fooMapping.Subclasses.Count());
        }

        [Test]
        public void Should_not_add_subclassmap_that_does_not_implement_parent_base()
        {
            /* The Parent is the FooBase class the desired results 
             * of this test is the exclusion of the StandAlone class 
             * since it does not implement the interface.
             */

            fooMapping = ((IMappingProvider)new BaseMap()).GetClassMapping();

            providers.Add(new StandAloneMap());
            var sut = CreateSut();
            sut.ProcessClass(fooMapping);
            Assert.AreEqual(0, fooMapping.Subclasses.Count());
        }

        [Test]
        public void Should_not_add_subclassmap_that_implements_a_subclass_of_the_parent_interface()
        {
            /* The Parent is the IFoo interface the desired results 
             * of this test is the inclusion of the BaseImpl class and 
             * the exclusion of the Foo<T> class since it implements 
             * the BaseImpl class which already implements FooBase.
             */

            fooMapping = ((IMappingProvider)new FooMap()).GetClassMapping();

            providers.Add(new BaseImplMap());
            providers.Add(new StringFooMap());
            var sut = CreateSut();
            sut.ProcessClass(fooMapping);
            Assert.AreEqual(1, fooMapping.Subclasses.Count());
            Assert.AreEqual(1, fooMapping.Subclasses.Where(sub => sub.Type.Equals(typeof(BaseImpl))).Count());
        }

        [Test]
        public void Should_not_add_subclassmap_that_implements_a_subclass_of_the_parent_base()
        {
            /* The Parent is the FooBase class the desired results 
             * of this test is the inclusion of the BaseImpl class and 
             * the exclusion of the Foo<T> class since it implements 
             * the BaseImpl class which already implements FooBase.
             */

            fooMapping = ((IMappingProvider)new BaseMap()).GetClassMapping();

            providers.Add(new BaseImplMap());
            providers.Add(new StringFooMap());
            var sut = CreateSut();
            sut.ProcessClass(fooMapping);
            Assert.AreEqual(1, fooMapping.Subclasses.Count());
            Assert.AreEqual(1, fooMapping.Subclasses.Where(sub => sub.Type.Equals(typeof(BaseImpl))).Count());
        }

        private SeparateSubclassVisitor CreateSut()
        {
            return new SeparateSubclassVisitor(providers);
        }


        private interface IFoo
        { }

        private class Base : IFoo
        { }

        private abstract class BaseImpl : Base
        { }

        private class Foo<T> : BaseImpl, IFoo
        { }

        private class FooMap : ClassMap<IFoo>
        { }

        private class BaseMap : ClassMap<Base>
        { }

        private class BaseImplMap : SubclassMap<BaseImpl>
        { }

        private abstract class GenericFooMap<T> : SubclassMap<Foo<T>>
        { }

        private class StringFooMap : GenericFooMap<string>
        { }


        private interface IStand
        { }

        private class StandAlone : IStand
        { }

        private class StandAloneMap : SubclassMap<StandAlone>
        { }
    }
}