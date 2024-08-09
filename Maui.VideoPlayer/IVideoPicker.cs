using System;
using System.Threading.Tasks;

namespace Maui.VideoPlayer
{
    public interface IVideoPicker
    {
        Task<string> GetVideoFileAsync();
    }
}
