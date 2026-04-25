using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.UnitTests.WebHost.Controllers.Partners.Shared;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.PromoCodes;
using System.Linq.Expressions;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.PromoCodes;

public class CreateTests
{
    private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
    private readonly Mock<IRepository<Preference>> _preferencesRepositoryMock;
    private readonly Mock<IRepository<PromoCode>> _promoCodesRepositoryMock;
    private readonly Mock<IRepository<Customer>> _customersRepositoryMock;
    private readonly Mock<IRepository<CustomerPromoCode>> _customerPromoCodesRepositoryMock;
    private readonly CancellationToken _ctNone = CancellationToken.None;
    private readonly PromoCodesController _controller;

    public CreateTests()
    {
        _partnersRepositoryMock = new Mock<IRepository<Partner>>();
        _preferencesRepositoryMock = new Mock<IRepository<Preference>>();
        _customerPromoCodesRepositoryMock = new Mock<IRepository<CustomerPromoCode>>();
        _promoCodesRepositoryMock = new Mock<IRepository<PromoCode>>();
        _customersRepositoryMock = new Mock<IRepository<Customer>>();

        _controller = new PromoCodesController(_promoCodesRepositoryMock.Object, _customersRepositoryMock.Object,
            _customerPromoCodesRepositoryMock.Object, _partnersRepositoryMock.Object,
            _preferencesRepositoryMock.Object);
    }

    [Fact]
    public async Task Create_WhenPartnerNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = PromoCodeCreateRequestCreater.Create(partnerId, preferenceId);

        Partner? returnedPartner = null;

        _partnersRepositoryMock
            .Setup(x => x.GetById(partnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartner);

        // Act
        var result = await _controller.Create(request, _ctNone);

        // Assert
        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFound.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Partner not found");

        _promoCodesRepositoryMock.Verify(x => x.Add(It.IsAny<PromoCode>(), _ctNone), Times.Never);
        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Never);
        _customersRepositoryMock.Verify(x => x.GetWhere(It.IsAny<Expression<Func<Customer, bool>>>(),
            withIncludes: false, ct: _ctNone), Times.Never);
    }

    [Fact]
    public async Task Create_WhenPreferenceNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = PromoCodeCreateRequestCreater.Create(partnerId, preferenceId);

        Partner? returnedPartner = PartnerCreater.Create(partnerId, isActive: true);
        Preference? returnedPreference = null;

        _partnersRepositoryMock
            .Setup(x => x.GetById(request.PartnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartner);

        _preferencesRepositoryMock
            .Setup(x => x.GetById(request.PreferenceId, withIncludes: false, ct: _ctNone))
            .ReturnsAsync(returnedPreference);

        // Act
        var result = await _controller.Create(request, _ctNone);

        // Assert
        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFound.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Preference not found");

        _promoCodesRepositoryMock.Verify(x => x.Add(It.IsAny<PromoCode>(), _ctNone), Times.Never);
        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Never);
        _customersRepositoryMock.Verify(x => x.GetWhere(It.IsAny<Expression<Func<Customer, bool>>>(),
            withIncludes: false, ct: _ctNone), Times.Never);
    }

    [Fact]
    public async Task Create_WhenNoActiveLimit_ReturnsUnprocessableEntity()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = PromoCodeCreateRequestCreater.Create(partnerId, preferenceId);
        var limitId = Guid.NewGuid();
        Partner? returnedPartnerNoActiveLimit = PartnerCreater.CreateWithCanceledLimit(partnerId, limitId, isActive: true,
            canceledAt: DateTimeOffset.UtcNow);

        Preference? returnedPreference = PreferenceCreater.CreateWithoutCustomers(preferenceId);
        List<Customer> customers = [];

        _partnersRepositoryMock
            .Setup(x => x.GetById(request.PartnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartnerNoActiveLimit);

        _preferencesRepositoryMock
            .Setup(x => x.GetById(request.PreferenceId, withIncludes: false, ct: _ctNone))
            .ReturnsAsync(returnedPreference);

        _customersRepositoryMock
            .Setup(x => x.GetWhere(It.IsAny<Expression<Func<Customer, bool>>>(), withIncludes: false, ct: _ctNone))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.Create(request, _ctNone);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("No active limit");

        _promoCodesRepositoryMock.Verify(x => x.Add(It.IsAny<PromoCode>(), _ctNone), Times.Never);
        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Never);
    }

    [Fact]
    public async Task Create_WhenLimitExceeded_ReturnsUnprocessableEntity()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = PromoCodeCreateRequestCreater.Create(partnerId, preferenceId);
        var limitId = Guid.NewGuid();
        Partner? returnedPartnerWithLimitExceeded = PartnerCreater.CreateWithLimitExceeded(partnerId, limitId);

        Preference? returnedPreference = PreferenceCreater.CreateWithoutCustomers(preferenceId);
        List<Customer> customers = [];

        _partnersRepositoryMock
            .Setup(x => x.GetById(request.PartnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartnerWithLimitExceeded);

        _preferencesRepositoryMock
            .Setup(x => x.GetById(request.PreferenceId, withIncludes: false, ct: _ctNone))
            .ReturnsAsync(returnedPreference);

        _customersRepositoryMock
            .Setup(x => x.GetWhere(It.IsAny<Expression<Func<Customer, bool>>>(), withIncludes: false, ct: _ctNone))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.Create(request, _ctNone);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Limit exceeded");

        _promoCodesRepositoryMock.Verify(x => x.Add(It.IsAny<PromoCode>(), _ctNone), Times.Never);
        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Never);
    }

    [Fact]
    public async Task Create_WhenValidRequest_ReturnsCreatedAndIncrementsIssuedCount()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var request = PromoCodeCreateRequestCreater.Create(partnerId, preferenceId);
        var limitId = Guid.NewGuid();
        Partner? partnerWithOneActiveLimit = PartnerCreater.CreateWithOneActiveLimit(partnerId, limitId, true);
        var oldValueIssuedCount = partnerWithOneActiveLimit.PartnerLimits.ElementAt(0).IssuedCount;

        Preference? returnedPreference = PreferenceCreater.CreateWithoutCustomers(preferenceId);
        List<Customer> customers = [];

        PromoCode? newPromoCode = null;
        Partner? updatedPartnerWithLimit = null;

        _partnersRepositoryMock
            .Setup(x => x.GetById(request.PartnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(partnerWithOneActiveLimit);

        _preferencesRepositoryMock
            .Setup(x => x.GetById(request.PreferenceId, withIncludes: false, ct: _ctNone))
            .ReturnsAsync(returnedPreference);

        _customersRepositoryMock
            .Setup(x => x.GetWhere(It.IsAny<Expression<Func<Customer, bool>>>(), withIncludes: false, ct: _ctNone))
            .ReturnsAsync(customers);

        _promoCodesRepositoryMock
            .Setup(x => x.Add(It.IsAny<PromoCode>(), _ctNone))
            .Callback<PromoCode, CancellationToken>((promoCode, _) => newPromoCode = promoCode);

        _partnersRepositoryMock
            .Setup(x => x.Update(It.IsAny<Partner>(), _ctNone))
            .Callback<Partner, CancellationToken>((partner, _) => updatedPartnerWithLimit = partner);

        // Act
        var result = await _controller.Create(request, _ctNone);

        // Assert
        newPromoCode.Should().NotBeNull();

        updatedPartnerWithLimit.Should().NotBeNull();
        var newValueIssuedCount = updatedPartnerWithLimit.PartnerLimits.ElementAt(0).IssuedCount;
        newValueIssuedCount.Should().Be(oldValueIssuedCount + 1);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be("GetById");

        var routeDict = created.RouteValues.Should().BeOfType<RouteValueDictionary>().Subject;
        routeDict["id"].Should().Be(newPromoCode.Id);

        var obj = created.Value.Should().BeOfType<PromoCodeShortResponse>().Subject;
        obj.Should().BeEquivalentTo(PromoCodesMapper.ToPromoCodeShortResponse(newPromoCode));

        _promoCodesRepositoryMock.Verify(x => x.Add(It.IsAny<PromoCode>(), _ctNone), Times.Once);
        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Once);
    }
}
