using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Translation.Domain.Entities;
using Translation.Infrastructure.Repositories;
using translation_app.Controllers;
using translation_app.Dto;

namespace Translation.API.Tests
{
    [TestFixture]
    public class TranslationControllerTests
    {
        private Mock<ITextResourceRepository> _repositoryMock;
        private TranslationController _controller;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<ITextResourceRepository>();

            _controller = new TranslationController(_repositoryMock.Object);
        }

        [Test]
        public async Task List_ReturnsOkWithResources()
        {
            var entities = new List<TextResourceEntity>
            {
                CreateEntity("SID_1", ("en", "Hello")),
                CreateEntity("SID_2", ("en", "World"))
            } as IEnumerable<TextResourceEntity>;

            _repositoryMock
                .Setup(r => r.GetAllTextResource())
                .ReturnsAsync(entities);

            var result = await _controller.List();

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);

            var dtos = ok!.Value as IEnumerable<TextResourceResponse>;
            Assert.That(dtos, Is.Not.Null);

            var list = dtos!.ToList();
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0].Sid, Is.EqualTo("SID_1"));
            Assert.That(list[0].Translations.First().LangId, Is.EqualTo("en"));
            Assert.That(list[0].Translations.First().Text, Is.EqualTo("Hello"));

            Assert.That(list[1].Sid, Is.EqualTo("SID_2"));
            Assert.That(list[1].Translations.First().LangId, Is.EqualTo("en"));
            Assert.That(list[1].Translations.First().Text, Is.EqualTo("World"));
        }


        [Test]
        public async Task Get_WhenResourceExists_ReturnsOk()
        {
            var entity = CreateEntity("SID_1", ("en", "Hello"));

            _repositoryMock
                .Setup(r => r.GetBySidAsync("SID_1"))
                .ReturnsAsync(entity);

            var result = await _controller.Get("SID_1");

            var ok = result.Result as OkObjectResult;
            var dto = ok!.Value as TextResourceResponse;

            Assert.That(ok, Is.Not.Null);
            Assert.That(dto!.Sid, Is.EqualTo("SID_1"));
            Assert.That(dto.Translations.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Get_WhenResourceDoesNotExist_ReturnsNotFound()
        {
            _repositoryMock
                .Setup(r => r.GetBySidAsync("MISSING"))
                .ReturnsAsync((TextResourceEntity?)null);

            var result = await _controller.Get("MISSING");

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }


        [Test]
        public async Task Create_WhenSidDoesNotExist_ReturnsCreated()
        {
            var request = new CreateTranslationRequest("SID_NEW", "Hello");

            _repositoryMock
                .Setup(r => r.GetBySidAsync("SID_NEW"))
                .ReturnsAsync((TextResourceEntity?)null);

            var result = await _controller.Create(request);

            var created = result.Result as CreatedAtActionResult;
            var dto = created!.Value as TextResourceResponse;

            Assert.That(created, Is.Not.Null);
            Assert.That(dto!.Sid, Is.EqualTo("SID_NEW"));

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TextResourceEntity>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Create_WhenSidAlreadyExists_ReturnsConflict()
        {
            _repositoryMock
                .Setup(r => r.GetBySidAsync("SID_1"))
                .ReturnsAsync(CreateEntity("SID_1"));

            var result = await _controller.Create(new CreateTranslationRequest("SID_1", "Text"));

            Assert.That(result.Result, Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task Update_WhenResourceExists_ReturnsNoContent()
        {
            var entity = CreateEntity("SID_1");

            _repositoryMock
                .Setup(r => r.GetBySidAsync("SID_1"))
                .ReturnsAsync(entity);

            var result = await _controller.Update(
                "SID_1",
                "fr",
                new UpdateTranslationRequest("Bonjour"));

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Update_WhenResourceDoesNotExist_ReturnsNotFound()
        {
            _repositoryMock
                .Setup(r => r.GetBySidAsync("SID_404"))
                .ReturnsAsync((TextResourceEntity?)null);

            var result = await _controller.Update(
                "SID_404",
                "en",
                new UpdateTranslationRequest("Hello"));

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_WhenResourceExists_ReturnsNoContent()
        {
            var entity = CreateEntity("SID_1");

            _repositoryMock
                .Setup(r => r.GetBySidAsync("SID_1"))
                .ReturnsAsync(entity);

            var result = await _controller.Delete("SID_1");

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _repositoryMock.Verify(r => r.DeleteBySidAsync("SID_1"), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Delete_WhenResourceDoesNotExist_ReturnsNotFound()
        {
            _repositoryMock
                .Setup(r => r.GetBySidAsync("SID_404"))
                .ReturnsAsync((TextResourceEntity?)null);

            var result = await _controller.Delete("SID_404");

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        private static TextResourceEntity CreateEntity(
            string sid,
            params (string lang, string text)[] translations)
        {
            var entity = new TextResourceEntity(sid);

            foreach (var (lang, text) in translations)
            {
                entity.AddOrUpdateTranslation(lang, text);
            }

            return entity;
        }
    }
}
