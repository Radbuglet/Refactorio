using Godot;

namespace Refactorio.game.simulation.ui
{
    public class PauseButton : Button
    {
        private void UpdateText(SceneTree tree)
        {
            Text = tree.Paused ? "Unpause simulation" : "Pause simulation";
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