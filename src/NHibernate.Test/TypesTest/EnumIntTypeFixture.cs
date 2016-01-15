using System;
using System.Collections;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	[TestFixture]
	public class EnumIntTypeFixture : TypeFixtureBase
	{
		protected override string TypeName
		{
			get { return "EnumInt"; }
		}

		protected override void OnSetUp()
		{
            EnumIntClass basic = new EnumIntClass();
			basic.Id = 1;
			basic.EnumValue = SampleEnum.Dimmed;

            EnumIntClass basic2 = new EnumIntClass();
			basic2.Id = 2;
			basic2.EnumValue = SampleEnum.On;

			ISession s = OpenSession();
			s.Save(basic);
			s.Save(basic2);
			s.Flush();
			s.Close();
		}

		protected override void OnTearDown()
		{
			ISession s = OpenSession();
			s.Delete("from EnumIntClass");
			s.Flush();
			s.Close();
		}


		[Test]
		public void ReadFromLoad()
		{
			ISession s = OpenSession();

            EnumIntClass basic = (EnumIntClass)s.Load(typeof(EnumIntClass), 1);
			Assert.AreEqual(SampleEnum.Dimmed, basic.EnumValue);

            EnumIntClass basic2 = (EnumIntClass)s.Load(typeof(EnumIntClass), 2);
			Assert.AreEqual(SampleEnum.On, basic2.EnumValue);

			s.Close();
		}


        [Test]
        public void Batch()
        {
            ISession s = OpenSession();

            for (int i = 0; i < 100; i++)
            {
                var tmp = new EnumIntClass
                    {
                        Id = /*bo 2 id 1 i 2 */ i,
                        EnumValue = SampleEnum.On
                    };
                s.Save(tmp);
            }
            s.Close();
        }
	}
}