using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Performance.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class IamBenchmarks
{
    private TenantId _tenantId = null!;
    private Email _email = null!;
    private User _user = null!;
    private List<User> _users = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _tenantId = TenantId.Create("tenant-perf-001");
        _email = Email.Create("benchmark@obss-test.com");
        _user = CreateTestUser("benchmark");
        _users = [];
        for (var i = 0; i < 1000; i++)
        {
            _users.Add(CreateTestUser($"seed-user-{i}"));
        }
    }

    [Benchmark]
    public User CreateUser()
    {
        var tenantId = TenantId.Create("tenant-perf-001");
        var email = Email.Create($"user-{Guid.NewGuid():N}@obss-test.com");
        return User.Create(
            tenantId,
            $"user-{Guid.NewGuid():N}",
            email,
            "Performance",
            "User");
    }

    [Benchmark]
    public User ActivateUser()
    {
        var user = CreateTestUser("activate-test");
        user.Activate();
        return user;
    }

    [Benchmark]
    public User DeactivateUser()
    {
        var user = CreateTestUser("deactivate-test");
        user.Deactivate();
        return user;
    }

    [Benchmark]
    public User AssignMultipleRoles()
    {
        var user = CreateTestUser("role-test");
        var assignedBy = Guid.NewGuid();
        for (var i = 0; i < 5; i++)
        {
            user.AssignRole(Guid.NewGuid(), assignedBy);
        }
        return user;
    }

    [Benchmark]
    public User UpdateProfile()
    {
        var phone = PhoneNumber.Create("+967712345678", "+967");
        _user.UpdateProfile("Updated", "Name", phone);
        _user.SetLastLogin();
        return _user;
    }

    [Benchmark]
    public User VerifyEmail()
    {
        var user = CreateTestUser("verify-test");
        user.VerifyEmail();
        return user;
    }

    private static User CreateTestUser(string prefix)
    {
        var tenantId = TenantId.Create("tenant-perf-001");
        var email = Email.Create($"{prefix}@obss-test.com");
        return User.Create(
            tenantId,
            $"{prefix}-{Guid.NewGuid():N}",
            email,
            "Test",
            "User");
    }
}
