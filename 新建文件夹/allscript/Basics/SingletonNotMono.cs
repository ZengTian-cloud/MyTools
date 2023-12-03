public class SingletonNotMono<T> where T : new()
{
    private static T m_Instance;
	private static object m_Lock = new object();
	public static T Instance
    {
		get
		{
			lock (m_Lock)
			{
                if (m_Instance == null)
                {
                    m_Instance = new T();
                }
                return m_Instance;
            }
		}
	}
}
