using FluentAssertions;
using System.IO;
using Xunit;

namespace Mbc.Hdf5Utils.Test
{
    public class H5DataSetTest
    {
        public H5DataSetTest()
        {
        }

        [Fact]
        public void OpenDataSet()
        {
            // Arrange
            var file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.Delete(file);
            using (var h5file = new H5File(file, H5File.Flags.CreateOnly))
            {
                new H5DataSet.Builder()
                    .WithName("foo")
                    .WithType<float>()
                    .WithDimension(100)
                    .WithChunking(10)
                    .Create(h5file);
            }

            // Act
            using (var h5file = new H5File(file, H5File.Flags.ReadOnly))
            {
                var dataSet = H5DataSet.Open(h5file, "foo");

                // Assert
                dataSet.Should().NotBeNull();
                dataSet.ValueType.Should().Be(typeof(float));
                dataSet.GetDimensions().Should().BeEquivalentTo(new ulong[] { 100 });
                dataSet.GetMaxDimensions().Should().BeEquivalentTo(new ulong[] { 100 });
                dataSet.GetChunkSize().Should().BeEquivalentTo(new ulong[] { 10 });
            }

        }
    }
}
