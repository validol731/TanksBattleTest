using UniRx;
using UnityEngine;

namespace UI
{
    public enum GameMenuType
    {
        MainMenu,
        Game
    }

    public class GameMenusController : MonoBehaviour
    {
        private readonly ReactiveProperty<GameMenuType> _current = new(GameMenuType.MainMenu);
        public IReadOnlyReactiveProperty<GameMenuType> Current => _current;

        private void Start()
        {
            SetMenu(GameMenuType.MainMenu);
        }

        public void SetMenu(GameMenuType type)
        {
            if (_current.Value != type)
            {
                _current.Value = type;
            }
        }
    }
}