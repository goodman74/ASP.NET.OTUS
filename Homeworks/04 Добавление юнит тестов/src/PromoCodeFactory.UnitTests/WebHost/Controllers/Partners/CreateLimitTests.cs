using AwesomeAssertions;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.Core.Exceptions;
using PromoCodeFactory.UnitTests.WebHost.Controllers.Partners.Shared;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.Partners;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners;

public class CreateLimitTests
{
    private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
    private readonly Mock<IRepository<PartnerPromoCodeLimit>> _partnerLimitsRepositoryMock;
    private readonly CancellationToken _ctNone = CancellationToken.None;
    private readonly PartnersController _controller;

    public CreateLimitTests()
    {
        _partnersRepositoryMock = new Mock<IRepository<Partner>>();
        _partnerLimitsRepositoryMock = new Mock<IRepository<PartnerPromoCodeLimit>>();
        _controller = new PartnersController(_partnersRepositoryMock.Object, _partnerLimitsRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateLimit_WhenPartnerNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        const int limit = 1;
        var request = new PartnerPromoCodeLimitCreateRequest(DateTime.UtcNow, limit);

        Partner? returnedPartner = null;

        _partnersRepositoryMock
            .Setup(x => x.GetById(partnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartner);

        // Act
        var result = await _controller.CreateLimit(partnerId, request, _ctNone);

        // Assert
        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFound.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Partner not found");

        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Never);
        _partnerLimitsRepositoryMock.Verify(x => x.Add(It.IsAny<PartnerPromoCodeLimit>(),_ctNone), Times.Never);
    }

    [Fact]
    public async Task CreateLimit_WhenPartnerBlocked_ReturnsUnprocessableEntity()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        const int limit = 1;
        var request = new PartnerPromoCodeLimitCreateRequest(DateTime.UtcNow, limit);

        var returnedPartner = PartnerCreater.Create(partnerId, isActive: false);

        _partnersRepositoryMock
            .Setup(x => x.GetById(partnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartner);

        // Act
        var result = await _controller.CreateLimit(partnerId, request, _ctNone);

        // Assert
        var unprocessable = result.Result.Should().BeOfType<UnprocessableEntityObjectResult>().Subject;
        var problemDetails = unprocessable.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Partner blocked");

        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Never);
        _partnerLimitsRepositoryMock.Verify(x => x.Add(It.IsAny<PartnerPromoCodeLimit>(), _ctNone), Times.Never);
    }

    [Fact]
    public async Task CreateLimit_WhenValidRequest_ReturnsCreatedAndAddsLimit()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var newLimit = new Randomizer().Int(1, 10);
        var endAtLimits = DateTime.UtcNow.AddDays(1);

        var request = new PartnerPromoCodeLimitCreateRequest(endAtLimits, newLimit);

        var returnedPartnerWithoutActiveLimits = PartnerCreater.Create(partnerId, isActive: true);

        PartnerPromoCodeLimit? newPartnerPromoCodeLimit = null;

        _partnersRepositoryMock
            .Setup(x => x.GetById(partnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartnerWithoutActiveLimits);

        _partnerLimitsRepositoryMock
            .Setup(x => x.Add(It.IsAny<PartnerPromoCodeLimit>(), _ctNone))
            .Callback<PartnerPromoCodeLimit, CancellationToken>((limit, _) => newPartnerPromoCodeLimit = limit);

        // Act
        var result = await _controller.CreateLimit(partnerId, request, _ctNone);

        // Assert
        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;

        created.ActionName.Should().Be("GetLimit");

        newPartnerPromoCodeLimit.Should().NotBeNull();
        newPartnerPromoCodeLimit.Limit.Should().Be(newLimit);
        newPartnerPromoCodeLimit.EndAt.Should().Be(endAtLimits);

        var routeDict = created.RouteValues.Should().BeOfType<RouteValueDictionary>().Subject;
        routeDict["partnerId"].Should().Be(partnerId);
        routeDict["limitId"].Should().Be(newPartnerPromoCodeLimit.Id);

        var obj = created.Value.Should().BeOfType<PartnerPromoCodeLimitResponse>().Subject;
        obj.Should().BeEquivalentTo(PartnersMapper.ToPartnerPromoCodeLimitResponse(newPartnerPromoCodeLimit));
    }

    [Fact]
    public async Task CreateLimit_WhenValidRequestWithActiveLimit_CancelsOldLimitsAndAddsNew()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var limitId = Guid.NewGuid();
        var returnedPartnerWithActiveLimit = PartnerCreater.CreateWithOneActiveLimit(partnerId, limitId, isActive: true);

        var newLimit = new Randomizer().Int(1, 10);
        var endAtLimits = DateTime.UtcNow.AddDays(1);

        var request = new PartnerPromoCodeLimitCreateRequest(endAtLimits, newLimit);

        PartnerPromoCodeLimit? newPartnerPromoCodeLimit = null;
        Partner? updatedPartner = null;

        _partnersRepositoryMock
            .Setup(x => x.GetById(partnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartnerWithActiveLimit);

        _partnersRepositoryMock
            .Setup(x => x.Update(It.IsAny<Partner>(), _ctNone))
            .Callback<Partner, CancellationToken>((partner, _) => updatedPartner = partner);

        _partnerLimitsRepositoryMock
            .Setup(x => x.Add(It.IsAny<PartnerPromoCodeLimit>(), _ctNone))
            .Callback<PartnerPromoCodeLimit, CancellationToken>((limit, _) => newPartnerPromoCodeLimit = limit);

        // Act
        var result = await _controller.CreateLimit(partnerId, request, _ctNone);

        // Assert
        updatedPartner.Should().NotBeNull();
        updatedPartner.PartnerLimits.Should().OnlyContain(l => l.CanceledAt != null);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;

        created.ActionName.Should().Be("GetLimit");

        newPartnerPromoCodeLimit.Should().NotBeNull();
        newPartnerPromoCodeLimit.Limit.Should().Be(newLimit);
        newPartnerPromoCodeLimit.EndAt.Should().Be(endAtLimits);
        newPartnerPromoCodeLimit!.CanceledAt.Should().BeNull();

        var routeDict = created.RouteValues.Should().BeOfType<RouteValueDictionary>().Subject;
        routeDict["partnerId"].Should().Be(partnerId);
        routeDict["limitId"].Should().Be(newPartnerPromoCodeLimit.Id);

        var obj = created.Value.Should().BeOfType<PartnerPromoCodeLimitResponse>().Subject;
        obj.Should().BeEquivalentTo(PartnersMapper.ToPartnerPromoCodeLimitResponse(newPartnerPromoCodeLimit));

        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Once);
        _partnerLimitsRepositoryMock.Verify(x => x.Add(It.IsAny<PartnerPromoCodeLimit>(), _ctNone), Times.Once);
    }

    [Fact]
    public async Task CreateLimit_WhenUpdateThrowsEntityNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var limitId = Guid.NewGuid();
        var returnedPartnerWithActiveLimit = PartnerCreater.CreateWithOneActiveLimit(partnerId, limitId, isActive: true);

        var newLimit = new Randomizer().Int(1, 10);
        var endAtLimits = DateTime.UtcNow.AddDays(1);

        var request = new PartnerPromoCodeLimitCreateRequest(endAtLimits, newLimit);

        _partnersRepositoryMock
            .Setup(x => x.GetById(partnerId, withIncludes: true, _ctNone))
            .ReturnsAsync(returnedPartnerWithActiveLimit);

        _partnersRepositoryMock
            .Setup(x => x.Update(It.IsAny<Partner>(), _ctNone))
            .ThrowsAsync(new EntityNotFoundException<Partner>(partnerId));

        // Act
        var result = await _controller.CreateLimit(partnerId, request, _ctNone);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();

        _partnersRepositoryMock.Verify(x => x.Update(It.IsAny<Partner>(), _ctNone), Times.Once);
        _partnerLimitsRepositoryMock.Verify(x => x.Add(It.IsAny<PartnerPromoCodeLimit>(), _ctNone), Times.Never);
    }
}
