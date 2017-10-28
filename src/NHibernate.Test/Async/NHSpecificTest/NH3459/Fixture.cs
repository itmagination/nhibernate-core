﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3459
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Order>(rc =>
				{
					rc.Table("Orders");
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Name);
					rc.Set(x => x.OrderLines, m =>
						{
							m.Inverse(true);
							m.Key(k =>
								{
									k.Column("OrderId");
									k.NotNullable(true);
								});
							m.Cascade(Mapping.ByCode.Cascade.All.Include(Mapping.ByCode.Cascade.DeleteOrphans));
							m.Access(Accessor.NoSetter);
						}, m => m.OneToMany());
				});
			mapper.Class<OrderLine>(rc =>
				{
					rc.Table("OrderLines");
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Manufacturer);
					rc.ManyToOne(x => x.Order, m => m.Column("OrderId"));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var o1 = new Order {Name = "Order 1"};
				session.Save(o1);

				var o2 = new Order {Name = "Order 2"};
				session.Save(o2);

				session.Save(new OrderLine {Manufacturer = "Manufacturer 1", Order = o2});

				var o3 = new Order {Name = "Order 3"};
				session.Save(o3);

				session.Save(new OrderLine {Manufacturer = "Manufacturer 1", Order = o3});
				session.Save(new OrderLine {Manufacturer = "Manufacturer 2", Order = o3});
				session.Save(new OrderLine {Manufacturer = "Manufacturer 3", Order = o3});

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public async Task LeftOuterJoinAndGroupByAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = await ((from o in session.Query<Order>()
							  from ol in o.OrderLines.DefaultIfEmpty()
							  group ol by ol.Manufacturer into grp
							  select new {grp.Key}).ToListAsync());

				Assert.AreEqual(4, result.Count);
			}
		}

		[Test]
		public async Task LeftOuterJoinWithInnerRestrictionAndGroupByAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = await ((from o in session.Query<Order>()
							  from ol in o.OrderLines.Where(x => x.Manufacturer == "Manufacturer 1").DefaultIfEmpty()
							  group o by o.Name into grp
							  select new {grp.Key}).ToListAsync());

				Assert.AreEqual(3, result.Count);
			}
		}

		[Test]
		public async Task LeftOuterJoinWithOuterRestrictionAndGroupByAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = await ((from o in session.Query<Order>()
							  from ol in o.OrderLines.DefaultIfEmpty().Where(x => x.Manufacturer == "Manufacturer 1")
							  group o by o.Name into grp
							  select new { grp.Key }).ToListAsync());

				Assert.AreEqual(2, result.Count);
			}
		}

		[Test]
		public async Task LeftOuterJoinWithOutermostRestrictionAndGroupByAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = await ((from o in session.Query<Order>()
							  from ol in o.OrderLines.DefaultIfEmpty()
							  where ol.Manufacturer == "Manufacturer 1"
							  group o by o.Name into grp
							  select new { grp.Key }).ToListAsync());

				Assert.AreEqual(2, result.Count);
			}
		}

		[Test]
		public async Task InnerJoinAndGroupByAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = await ((from o in session.Query<Order>()
							  from ol in o.OrderLines
							  group ol by ol.Manufacturer into grp
							  select new { grp.Key }).ToListAsync());

				Assert.AreEqual(3, result.Count);
			}
		}

		[Test]
		public async Task InnerJoinWithRestrictionAndGroupByAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = await ((from o in session.Query<Order>()
							  from ol in o.OrderLines.Where(x => x.Manufacturer == "Manufacturer 1")
							  group o by o.Name into grp
							  select new { grp.Key }).ToListAsync());

				Assert.AreEqual(2, result.Count);
			}
		}

		[Test]
		public async Task InnerJoinWithOutermostRestrictionAndGroupByAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = await ((from o in session.Query<Order>()
							  from ol in o.OrderLines
							  where ol.Manufacturer == "Manufacturer 1"
							  group o by o.Name into grp
							  select new { grp.Key }).ToListAsync());

				Assert.AreEqual(2, result.Count);
			}
		}
	}
}