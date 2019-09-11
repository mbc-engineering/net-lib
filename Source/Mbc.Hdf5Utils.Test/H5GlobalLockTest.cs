using FluentAssertions;
using Xunit;

namespace Mbc.Hdf5Utils.Test
{
    public class H5GlobalLockTest
    {
        public H5GlobalLockTest()
        {
        }

        [Fact]
        public void HasLockWithInstance()
        {
            using (new H5GlobalLock())
            {
                H5GlobalLock.HasLock.Should().BeTrue();
            }

            H5GlobalLock.HasLock.Should().BeFalse();
        }

        [Fact]
        public void HasLockWithLock()
        {
            lock (H5GlobalLock.Sync)
            {
                H5GlobalLock.HasLock.Should().BeTrue();
            }

            H5GlobalLock.HasLock.Should().BeFalse();
        }
    }
}
