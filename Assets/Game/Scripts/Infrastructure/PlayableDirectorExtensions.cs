using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Game.Scripts.Infrastructure
{
    public static class PlayableDirectorExtensions
    {
        public static async Task AwaitPlayableEnd(this PlayableDirector director)
        {
            if (director == null)
            {
                Debug.LogWarning("PlayableDirector is null.");
                return;
            }

            while (director.state != PlayState.Playing)
            {
                await Task.Yield();
            }

            while (director.state == PlayState.Playing)
            {
                await Task.Yield();
            }
        }
    }
}