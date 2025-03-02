using MapChanger.MonoBehaviours;

namespace RandoMapMod.Rooms
{
    internal abstract class RoomSelector : Selector
    {
        public override float SelectionRadius { get; } = 2.5f;

        public override float SpriteSize { get; } = 0.6f;

        internal virtual void Initialize(IEnumerable<MapObject> rooms)
        {
            base.Initialize();

            ActiveModifiers.AddRange(
            [
                ActiveByCurrentMode,
                ActiveByToggle
            ]);

            foreach (MapObject room in rooms)
            {
                string sceneName = "";
                if (room is RoomSprite roomSprite)
                {
                    sceneName = roomSprite.Rsd.SceneName;
                }
                if (room is RoomText roomText)
                {
                    sceneName = roomText.Rtd.Name;
                }

                if (Objects.TryGetValue(sceneName, out List<ISelectable> selectables))
                {
                    selectables.Add((ISelectable)room);
                }
                else
                {
                    Objects[sceneName] = [(ISelectable)room];
                }
            }
        }

        protected private abstract bool ActiveByCurrentMode();
        protected private abstract bool ActiveByToggle();

        public override void OnMainUpdate(bool active)
        {
            base.OnMainUpdate(active);

            SpriteObject.SetActive(RandoMapMod.GS.ShowReticle);
        }

        protected override void Select(ISelectable selectable)
        {
            selectable.Selected = true;
        }

        protected override void Deselect(ISelectable selectable)
        {
            selectable.Selected = false;
        }
    }
}
