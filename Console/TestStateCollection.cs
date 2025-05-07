using Korn.Utils.Algorithms;

static class TestStateCollection
{
    public static void Execute()
    {
        var collection = new StateCollection(0x1000);
        var i0 = collection.HoldEntry();
        var i1 = collection.HoldEntry();
        var i2 = collection.HoldEntry();

        collection.FreeEntry(i1);

        i1 = collection.HoldEntry();

        _ = 3;
    }
}