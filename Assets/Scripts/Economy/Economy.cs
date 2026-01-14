
public class Economy 
{
    private int _money;
    public int Money { get { return _money; } }

    public Economy(int startingMoney)
    {
        _money = startingMoney;
    }

    public void AddMoney(int amount)
    {
        _money += amount;
    }

    public bool HasEnoughMoney(int amount)
    {
        return _money >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (HasEnoughMoney(amount))
        {
            _money -= amount;
            return true;
        }
        return false;
    }
}