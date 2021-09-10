using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VaraniumSharp.Wrappers;
using Xunit;

namespace VaraniumSharp.Tests.Wrappers
{
    public class DirectoryWrapperTests
    {
        private class FolderHelperFixture
        {
            #region Constructor

            public FolderHelperFixture()
            {
                AppPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            #endregion

            #region Properties

            public string AppPath { get; }

            #endregion

            #region Public Methods

            public DirectoryWrapper GetInstance()
            {
                return new DirectoryWrapper();
            }

            #endregion
        }

        [Fact]
        public void DirectoryIsCorrectlyCreated()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var folderToCreate = Path.Combine(appPath, Guid.NewGuid().ToString());
            var fixture = new FolderHelperFixture();

            var sut = fixture.GetInstance();

            // act
            var result = sut.CreateDirectory(folderToCreate);

            // assert
            result.Exists.Should().BeTrue();
            Directory.Exists(folderToCreate).Should().BeTrue();

            Directory.Delete(folderToCreate);
        }

        [Fact]
        public void DirectoryIsCorrectlyEnumerated()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var folderToEnum = Path.Combine(appPath, "Resources", "DirectoryToEnumerate");
            var expectedFile1 = Path.Combine(folderToEnum, "File1.txt");
            var expectedFile2 = Path.Combine(folderToEnum, "File2.txt");
            var fixture = new FolderHelperFixture();

            var sut = fixture.GetInstance();

            // act
            var files = sut.EnumerateFiles(folderToEnum).ToList();

            // assert
            files.Count.Should().Be(2);
            files.Should().Contain(expectedFile1);
            files.Should().Contain(expectedFile2);
        }

        [Fact]
        public void IfFolderDoesNotExistFalseIsReturned()
        {
            // arrange
            var fixture = new FolderHelperFixture();
            var directoryName = Guid.NewGuid().ToString();
            var currentDirectory = Path.Combine(fixture.AppPath, directoryName);

            var sut = fixture.GetInstance();

            // act
            var result = sut.Exists(currentDirectory);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IfFolderExistsTrueIsReturned()
        {
            // arrange
            var fixture = new FolderHelperFixture();
            var directoryName = Guid.NewGuid().ToString();
            var currentDirectory = Path.Combine(fixture.AppPath, directoryName);

            Directory.CreateDirectory(currentDirectory);

            var sut = fixture.GetInstance();

            // act
            var result = sut.Exists(currentDirectory);

            // assert
            result.Should().BeTrue();
        }
    }
}