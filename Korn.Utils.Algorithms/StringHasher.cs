using Korn.Utils.Unsafe;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
public unsafe static class StringHasher
{
    public static ulong CalculateHash(string value)
    {
        var length = value.Length;
        var hashedValue = 3074457345618258791ul;
        for (var i = 0; i < length; i++)
        {
            hashedValue += value[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
}

public struct HashedString
{
    public HashedString(string value)
    {
        String = value;
        Hash = CalculateHash(value);
    }

    public readonly string String;
    public readonly ulong Hash;

    public static ulong CalculateHash(string value) => StringHasher.CalculateHash(value);
}