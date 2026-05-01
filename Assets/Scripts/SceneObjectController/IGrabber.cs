public interface IGrabber
{
    IDraggable CurrentTarget { get; }
    bool IsDragging { get; }
}