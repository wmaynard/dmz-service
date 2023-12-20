using System.Collections.Generic;
using System.Linq;
using Dmz.Models.Permissions;
using MongoDB.Driver.Linq;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Exceptions.Mongo;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Minq;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Services;

public class RoleService : MinqTimerService<Role>
{
	private readonly CacheService _cache;

	public RoleService(CacheService cache) : base("roles", IntervalMs.ThirtyMinutes)
	{
		mongo
			.DefineIndex(index => index
				.Add(role => role.Name)
				.EnforceUniqueConstraint()
			);
		_cache = cache;
	}

	public void EnsureNameNotTaken(string name)
	{
		if (mongo.Count(query => query.EqualTo(role => role.Name, name)) > 0)
			throw new PlatformException(message: "Role already exists.");
	}

	public Role FindByName(string name) => mongo.FirstOrDefault(query => query.EqualTo(role => role.Name, name));

	public Role[] FindByName(params string[] names) => mongo
		.Where(query => query.ContainedIn(role => role.Name, names))
		.ToArray();

	public Role[] FromIds(params string[] ids)
	{
		List<Role> output = new();
		const string rolePrefix = "role_";
		foreach (string roleId in ids)
			if (_cache.HasValue($"{rolePrefix}{roleId}", out Role temp))
				output.Add(temp);

		Role[] missing = mongo
			.Where(query => query.ContainedIn(role => role.Id, ids.Except(output.Select(role => role.Id))))
			.ToArray();
		
		foreach (Role role in missing)
			_cache.Store($"{rolePrefix}{role.Id}", role, IntervalMs.FifteenMinutes);
		output.AddRange(missing);
		return output.ToArray();
	}

	public Role[] List() => mongo
		.All()
		.ToArray();

	public string[] IdsFromPermission(string permissionName) => mongo
		.Where(query => query.Where(role => role.Permissions, subquery => subquery.Contains(group => group.Values, new KeyValuePair<string, bool>(permissionName, true))))
		.Project(role => role.Id);

	public void Delete(string id)
	{
		long affected = mongo
			.ExactId(id)
			.Delete();
		if (affected != 1)
			throw new RecordsAffectedException(min: 1, max: 1, affected);
	}

	public void EnsureAllRolesExist(string[] ids)
	{
		if (ids == null || ids.Length == 0)
			return;
		if (mongo.Count(query => query.ContainedIn(role => role.Id, ids.Where(id => id.CanBeMongoId()).ToArray())) != ids.Length)
			throw new PlatformException("At least one of the specified roles does not exist.");
	}

	public bool UpdatePermissions(string id, Passport passport) => mongo
		.ExactId(id)
		.Update(update => update.Set(role => role.Permissions, passport)) > 0;

	private const string SUPERUSER_NAME = "superuser";
	public void EnsureSuperUserExists()
	{
		Role superuser = new()
		{
			Name = SUPERUSER_NAME,
			Permissions = new Passport(Passport.PassportType.Superuser)
		};

		mongo
			.Where(query => query.EqualTo(role => role.Name, superuser.Name))
			.Upsert(update => update.Set(role => role.Permissions, superuser.Permissions));
	}

	public void EnforceIdNotSuperuser(string id)
	{
		string name = mongo
			.ExactId(id)
			.FirstOrDefault()?.Name;
		if (name == SUPERUSER_NAME)
			throw new PlatformException("You cannot change or delete the superuser role.");
	}

	protected override void OnElapsed() => EnsureSuperUserExists();
}