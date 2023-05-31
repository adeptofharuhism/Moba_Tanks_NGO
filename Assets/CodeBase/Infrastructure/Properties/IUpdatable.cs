namespace Assets.CodeBase.Infrastructure.Properties
{
    public interface IUpdatable
    {
        void HandleInput();
        void Update();
        void FixedUpdate();
    }
}
