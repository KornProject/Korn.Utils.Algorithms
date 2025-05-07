using Korn.Utils.Algorithms;

static unsafe class TestLinkedArray
{
    public static void Execute()
    {
        var array = new LinkedArray();

        var n1 = array.AddNode();
        n1->Value = 1;

        var n2 = array.AddNode();
        n2->Value = 2;

        var n3 = array.AddNode();
        n3->Value = 3;

        array.RemoveNode(n2);
        array.RemoveNode(n1);

        _ = 3;
    }
}