using Godot;

namespace Refactorio.game.simulation
{
    public class PauseButton : Button
    {
        private void UpdateText(SceneTree tree)
        {
            Text = tree.Paused ? "Unpause game" : "Pause game";
        }

        public override void _Ready()
        {
            UpdateText(GetTree());
        }

        public override void _Pressed()
        {
            var tree = GetTree();
            tree.Paused = !tree.Paused;
            UpdateText(tree);
        }
    }
}