using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Collections;
using VaraniumSharp.EventArguments;

namespace VaraniumSharp.Tests.Collections
{
    public class PackageManagerTests
    {
        #region Public Methods

        [Test]
        public async Task AddingDataWithTheSameInteralStoragePathAsExistingDataOverwritesTheExistingData()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(appPath, ResouceDirectory, "File1.txt");
            var filePath2 = Path.Combine(appPath, ResouceDirectory, "File2.txt");
            const string storagePath = "docs/File1.txt";

            File.Exists(packagePath).Should().BeFalse();

            var sut = new PackageManager();
            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream, storagePath);
            }

            // act
            using (var fileStream2 = File.Open(filePath2, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream2, storagePath);
            }

            // assert
            var stream = await sut.RetrieveDataFromPackageAsync(packagePath, storagePath);
            using (var reader = new StreamReader(stream))
            {
                reader.ReadLine().Should().Be("Overwriting data");
            }

            sut.Dispose();
            File.Delete(packagePath);
        }

        [Test]
        public async Task ItemIsAddedToThePackageCorrectly()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(appPath, ResouceDirectory, "File1.txt");
            const string storagePath = "docs/File1.txt";

            File.Exists(packagePath).Should().BeFalse();

            var sut = new PackageManager();

            // act
            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream, storagePath);
            }

            // assert
            sut.Dispose();
            var package = new ZipArchive(File.Open(packagePath, FileMode.Open), ZipArchiveMode.Read);
            package.Entries.Any(x => x.FullName == storagePath).Should().BeTrue();

            package.Dispose();
            File.Delete(packagePath);
        }

        [Test]
        public async Task RemovingDataFromThePackageCorrectlyRemoveIt()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(appPath, ResouceDirectory, "File1.txt");
            const string storagePath = "docs/File1.txt";

            File.Exists(packagePath).Should().BeFalse();

            var sut = new PackageManager();
            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream, storagePath);
            }

            // act
            await sut.RemoveDataFromPackageAsync(packagePath, storagePath);

            // assert
            sut.Dispose();
            var package = new ZipArchive(File.Open(packagePath, FileMode.Open), ZipArchiveMode.Read);
            package.Entries.Any(x => x.FullName == storagePath).Should().BeFalse();

            package.Dispose();
            File.Delete(packagePath);
        }

        [Test]
        public async Task ScrubbingPackageCorrectlyRemovesUnwantedDataFromThePackage()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(appPath, ResouceDirectory, "File1.txt");
            var filePath2 = Path.Combine(appPath, ResouceDirectory, "File2.txt");
            const string storagePath1 = "docs/File1.txt";
            const string storagePath2 = "docs/File2.txt";
            var listToKeep = new List<string> { storagePath1 };

            File.Exists(packagePath).Should().BeFalse();

            var sut = new PackageManager();
            using (var fileStream1 = File.Open(filePath, FileMode.Open))
            using (var fileStream2 = File.Open(filePath2, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream1, storagePath1);
                await sut.AddItemToPackageAsync(packagePath, fileStream2, storagePath2);
            }

            // act
            await sut.ScrubStorageAsync(packagePath, listToKeep);

            // assert
            sut.Dispose();
            var package = new ZipArchive(File.Open(packagePath, FileMode.Open), ZipArchiveMode.Read);
            package.Entries.Any(x => x.FullName == storagePath2).Should().BeFalse();

            package.Dispose();
            File.Delete(packagePath);
        }

        [Test]
        public async Task ScrubbingPackageCorrectlyReportsTheScrubbingProgress()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(appPath, ResouceDirectory, "File1.txt");
            var filePath2 = Path.Combine(appPath, ResouceDirectory, "File2.txt");
            const string storagePath1 = "docs/File1.txt";
            const string storagePath2 = "docs/File2.txt";
            var listToKeep = new List<string> { storagePath1 };
            var scrubProgress = new List<ScrubProgressArgs>();

            File.Exists(packagePath).Should().BeFalse();

            var sut = new PackageManager();
            using (var fileStream1 = File.Open(filePath, FileMode.Open))
            using (var fileStream2 = File.Open(filePath2, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream1, storagePath1);
                await sut.AddItemToPackageAsync(packagePath, fileStream2, storagePath2);
            }

            sut.ScrubProgress += (sender, args) =>
            {
                scrubProgress.Add(args);
            };

            // act
            await sut.ScrubStorageAsync(packagePath, listToKeep);
            await Task.Delay(100);

            // assert
            scrubProgress.Count.Should().Be(2);
            scrubProgress.First().PercentageCompleted.Should().Be(50);
            scrubProgress.Last().TotalEntries.Should().Be(2);
        }

        [Test]
        public async Task ScrubbingPackageDoesNotRemoveDataForPathsThatWereRequestedToBeLeft()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(appPath, ResouceDirectory, "File1.txt");
            var filePath2 = Path.Combine(appPath, ResouceDirectory, "File2.txt");
            const string storagePath1 = "docs/File1.txt";
            const string storagePath2 = "docs/File2.txt";
            var listToKeep = new List<string> { storagePath1 };

            File.Exists(packagePath).Should().BeFalse();

            var sut = new PackageManager();
            using (var fileStream1 = File.Open(filePath, FileMode.Open))
            using (var fileStream2 = File.Open(filePath2, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream1, storagePath1);
                await sut.AddItemToPackageAsync(packagePath, fileStream2, storagePath2);
            }

            // act
            await sut.ScrubStorageAsync(packagePath, listToKeep);

            // assert
            sut.Dispose();
            var package = new ZipArchive(File.Open(packagePath, FileMode.Open), ZipArchiveMode.Read);
            package.Entries.Any(x => x.FullName == storagePath1).Should().BeTrue();

            package.Dispose();
            File.Delete(packagePath);
        }

        [Test]
        public async Task StoredDataIsCorrectlyRetrievedFromThePackage()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(appPath, ResouceDirectory, "File1.txt");
            const string storagePath = "docs/File1.txt";

            File.Exists(packagePath).Should().BeFalse();

            var sut = new PackageManager();
            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream, storagePath);
            }

            // act
            var stream = await sut.RetrieveDataFromPackageAsync(packagePath, storagePath);

            // assert
            using (var reader = new StreamReader(stream))
            {
                reader.ReadLine().Should().Be("Test File 1");
            }

            sut.Dispose();
            File.Delete(packagePath);
        }

        [Test]
        public async Task WhenAnItemIsAddedToAPackageThatDoesNotExistThePackageIsCreated()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            var filePath = Path.Combine(appPath, ResouceDirectory, "File1.txt");
            const string storagePath = "docs/File1.txt";

            File.Exists(packagePath).Should().BeFalse();

            var sut = new PackageManager();

            // act
            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                await sut.AddItemToPackageAsync(packagePath, fileStream, storagePath);
            }

            // assert
            File.Exists(packagePath).Should().BeTrue();

            sut.Dispose();
            File.Delete(packagePath);
        }

        [Test]
        public void WhenAPackageDoesNotContainTheDesiredItemAnExceptionIsThrown()
        {
            // arrange
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var packagePath = Path.Combine(appPath, Guid.NewGuid().ToString());
            const string storagePath = "docs/File1.txt";

            var sut = new PackageManager();
            var act = new Action(() => sut.RetrieveDataFromPackageAsync(packagePath, storagePath).Wait());

            // act
            // assert
            act.ShouldThrow<KeyNotFoundException>();
        }

        #endregion

        #region Variables

        private const string ResouceDirectory = "Resources";

        #endregion
    }
}