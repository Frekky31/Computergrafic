using Rasterization.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rasterization.Scenes
{
    public class Scene
    {
        public List<Mesh> Meshes { get; } = [];
        public Scene()
        {
        }

        public virtual void Update(float delta)
        {

        }
    }
}
