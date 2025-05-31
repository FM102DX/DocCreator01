namespace DocCreator01.Messages
{
    // Sent via ReactiveUI.MessageBus when a text-part element was deleted,
    // carrying its index in the collection
    public sealed record TextPartCollectionElementDeletedMessage(int DeletedIndex);
}
