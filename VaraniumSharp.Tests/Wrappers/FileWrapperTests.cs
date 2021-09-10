using System.IO;
using System;
using System.Threading.Tasks;
using Xunit;
using VaraniumSharp.Wrappers;
using FluentAssertions;

namespace VaraniumSharp.Tests.Wrappers
{
    public class FileWrapperTests
    {
        [Fact]
        public void FileIsCorrectlyDeleted()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            if (appPath == null)
            {
                throw new InvalidOperationException("Cannot run unit test if AppPath is empty");
            }
            var filePath = Path.Combine(appPath, "Resources", Guid.NewGuid().ToString());
            File
                .Create(filePath)
                .Close();
            var sut = new FileWrapper();

            var file = File.Create(filePath);
            file.Close();

            // act
            sut.DeleteFile(filePath);

            // assert
            File.Exists(filePath).Should().BeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CheckingIfFileExistsWorksCorrectly(bool fileExists)
        {
            // arrange
            var fileName = Guid.NewGuid().ToString();

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (fileExists)
            {
                File
                    .Create(filePath)
                    .Close();
            }

            var sut = new FileWrapper();

            // act
            var result = sut.FileExists(filePath);

            // assert
            if (fileExists)
            {
                File.Delete(filePath);
            }
            result.Should().Be(fileExists);
        }

        [Fact]
        public void RetrievingFileInformationCorrectlyReturnsFileInformation()
        {
            // arrange
            var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\TestFileNew.mkv");
            var sut = new FileWrapper();

            // act
            var result = sut.GetFileInformation(location);

            // assert
            var externalInfo = new FileInfo(location);
            externalInfo.Should().NotBeNull();
            result.Folder.Should().Be(externalInfo.Directory?.FullName);
            result.Filename.Should().Be(externalInfo.Name);
            result.Size.Should().Be(externalInfo.Length);
        }

        [Fact]
        public void RetrievingFileSizeReturnsTheCorrectFileSize()
        {
            // arrange
            var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\TestFileNew.mkv");
            var sut = new FileWrapper();

            // act
            var size = sut.GetFileSize(location);

            // assert
            size.Should().Be(728973L);
        }

        [Fact]
        public void VersionIsCorrectlyRetrievedFromABinary()
        {
            // arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SmallLib.dll");

            var sut = new FileWrapper();

            // act
            var version = sut.GetFileVersion(filePath);

            // assert
            version.Should().Be("4.2.1.0");
        }

        [Fact]
        public void FileIsOpenedCorrectly()
        {
            // arrange
            var fileName = $"{Guid.NewGuid()}.txt";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            File
                .CreateText(filePath)
                .Close();
            var sut = new FileWrapper();

            // act
            var stream = sut.OpenFile(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            // assert
            stream.Should().NotBeNull();
            stream.Close();

            File.Delete(filePath);
        }

        [Fact]
        public void AllTextIsCorrectlyReadFromTheTextFile()
        {
            // arrange
            const string expectedText = "Hello\r\nWorld!";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TextFile.txt");
            File.WriteAllText(filePath, expectedText);

            var sut = new FileWrapper();

            // act
            var result = sut.ReadAllText(filePath);

            // assert
            result.Should().Be(expectedText);
        }

        [Fact]
        public async Task AllTextIsCorrectlyReadFromTheTextFileAsync()
        {
            // arrange
            const string expectedText = "Hello\r\nWorld!";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TextFile.txt");
            await File.WriteAllTextAsync(filePath, expectedText);

            var sut = new FileWrapper();

            // act
            var result = await sut.ReadAllTextAsync(filePath);

            // assert
            result.Should().Be(expectedText);
        }

        [Fact]
        public void RenamingFileCorrectlyRenamesTheSelectedFile()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = Guid.NewGuid().ToString();
            var newName = Guid.NewGuid().ToString();

            var filePath = Path.Combine(appPath, fileName);
            var targetPath = Path.Combine(appPath, newName);
            var file = File.Create(filePath);
            file.Close();

            var sut = new FileWrapper();

            // act
            sut.RenameFile(filePath, targetPath);

            // assert
            File.Exists(filePath).Should().BeFalse();
            File.Exists(targetPath).Should().BeTrue();

            File.Delete(targetPath);
        }

        [Fact]
        public void TextIsCorrectlyWrittenToFile()
        {
            // arrange
            const string textToWrite = "Hello World\r\nHope you are fine";
            var fileName = $"{Guid.NewGuid()}.txt";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            var sut = new FileWrapper();

            // act
            sut.WriteAllText(filePath, textToWrite);

            // assert
            File.Exists(filePath).Should().BeTrue();
            var fileText = File.ReadAllText(filePath);
            fileText.Should().Be(textToWrite);

            File.Delete(filePath);
        }

        [Fact]
        public async Task TextIsCorrectlyWrittenToFileAsync()
        {
            // arrange
            const string textToWrite = "Hello World\r\nHope you are fine";
            var fileName = $"{Guid.NewGuid()}.txt";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            var sut = new FileWrapper();

            // act
            await sut.WriteAllTextAsync(filePath, textToWrite);

            // assert
            File.Exists(filePath).Should().BeTrue();
            var fileText = await File.ReadAllTextAsync(filePath);
            fileText.Should().Be(textToWrite);

            File.Delete(filePath);
        }
    }
}
