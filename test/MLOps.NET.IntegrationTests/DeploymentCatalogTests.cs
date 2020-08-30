﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MLOps.NET.Docker;
using MLOps.NET.Docker.Settings;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MLOps.NET.IntegrationTests
{
    public class DeploymentCatalogTests : RepositoryTests
    {
        private string tagName;
        private DockerSettings dockerSettings;
        private CliExecutor cliExecutor;
        private readonly string experimentName = "MyExperiment";

        protected void SetUp()
        {
            this.dockerSettings = new DockerSettings
            {
                RegistryName = "localhost:5000"
            };

            this.tagName = $"{dockerSettings.RegistryName}/{experimentName}:1";
            cliExecutor = new CliExecutor(dockerSettings);
        }

        [TestMethod]
        public async Task CreateDeploymentTarget_GivenName_CreatesAValidDeploymentTarget()
        {
            //Act
            await sut.Deployment.CreateDeploymentTargetAsync("Production");

            //Assert
            var deploymentTarget = sut.Deployment.GetDeploymentTargets().FirstOrDefault(x => x.Name == "Production");
            deploymentTarget.Should().NotBeNull();

            deploymentTarget.CreatedDate.Date.Should().Be(DateTime.UtcNow.Date);
            deploymentTarget.IsProduction.Should().BeFalse();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Deployment target name was not specified")]
        public async Task CreateDeploymentTarget_GivenNameIsNullOrEmpty_ThrowsExceptionWithMessage()
        {
            //Act
            await sut.Deployment.CreateDeploymentTargetAsync(null);
        }

        [TestMethod]
        public async Task CreateDeployment_ShouldCreateDeployment()
        {
            //Arrange
            var run = await sut.LifeCycle.CreateRunAsync(this.experimentName);
            var runArtifact = await sut.Model.UploadAsync(run.RunId, @"Data/model.txt");
            var registeredModel = await sut.Model.RegisterModel(run.ExperimentId, runArtifact.RunArtifactId, "registerby");
            var deploymentTarget = await sut.Deployment.CreateDeploymentTargetAsync("Prod");

            //Act
            await sut.Deployment.DeployModelAsync(deploymentTarget, registeredModel, "By me");

            //Assert
            var deployment = sut.Deployment.GetDeployments(registeredModel.ExperimentId).First();

            deployment.DeployedBy.Should().Be("By me");
            deployment.DeploymentDate.Date.Should().Be(DateTime.UtcNow.Date);
        }

        [TestMethod]
        public async Task GetRegisteredModel_GivenARegisteredModelHasMoreThanOneDeployment_ShouldPopulateDeployments()
        {
            //Arrange
            var run = await sut.LifeCycle.CreateRunAsync(this.experimentName);
            var runArtifact = await sut.Model.UploadAsync(run.RunId, @"Data/model.txt");
            var registeredModel = await sut.Model.RegisterModel(run.ExperimentId, runArtifact.RunArtifactId, "registerby");

            var testDeploymentTarget = await sut.Deployment.CreateDeploymentTargetAsync("Test");
            var prodDeploymentTarget = await sut.Deployment.CreateDeploymentTargetAsync("Prod");

            await sut.Deployment.DeployModelAsync(testDeploymentTarget, registeredModel, "By me");
            await sut.Deployment.DeployModelAsync(prodDeploymentTarget, registeredModel, "By me");

            //Act
            registeredModel = sut.Model.GetLatestRegisteredModel(registeredModel.ExperimentId);

            //Assert
            registeredModel.Deployments.Should().NotBeNull();
            registeredModel.Deployments.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task DeployModelToContainerAsync_ShouldPushImageToRegistry()
        {
            //Arrange
            await cliExecutor.RemoveDockerImage(tagName);

            var run = await sut.LifeCycle.CreateRunAsync(this.experimentName);
            var runArtifact = await sut.Model.UploadAsync(run.RunId, @"Data/model.txt");
            var registeredModel = await sut.Model.RegisterModel(run.ExperimentId, runArtifact.RunArtifactId, "registerby");
            var deploymentTarget = await sut.Deployment.CreateDeploymentTargetAsync("Prod");

            //Act
            var deployment = await sut.Deployment.DeployModelToContainerAsync(deploymentTarget, registeredModel, "deployedBy");

            //Assert
            deployment.Should().NotBeNull();

            var imageExists = await cliExecutor.RunDockerPull(tagName);
            imageExists.Should().BeTrue();
        }

        [TestMethod]
        public async Task DeployModelToContainerAsync_ShouldBaseImageOfModelAndProject()
        {
            //Arrange
            await cliExecutor.RemoveDockerImage(tagName);

            var run = await sut.LifeCycle.CreateRunAsync(this.experimentName);
            var runArtifact = await sut.Model.UploadAsync(run.RunId, @"Data/model.txt");
            var registeredModel = await sut.Model.RegisterModel(run.ExperimentId, runArtifact.RunArtifactId, "registerby");
            var deploymentTarget = await sut.Deployment.CreateDeploymentTargetAsync("Prod");

            //Act
            await sut.Deployment.DeployModelToContainerAsync(deploymentTarget, registeredModel, "deployedBy");

            //Assert
            File.Exists("image/ML.NET.Web.Embedded.csproj").Should().BeTrue();
            File.Exists("image/model.zip").Should().BeTrue();
        }  
    }
}
