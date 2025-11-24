using OpenGL.Objects;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.Scenes
{
    public abstract class Scene
    {
        public SceneGraphNode Root {get; protected set; } = new SceneGraphNode("Root");
        public Scene() { }

        public abstract void LoadScene(int hProgram);
        public abstract void UpdateScene(int hProgram, float time);
        public abstract void RenderScene(int hProgram, float time, GameWindow w);
    }
}
