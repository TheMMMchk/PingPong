public class HealthModel
{
    private int _hp = 100;
    public int Hp
    {
        get => _hp;
        private set
        {
            if (value < 0)
                return;

            _hp = value;

            if (_hp > 0)
                return;

            _hp = 0;

        }
    }

    public HealthModel(int hp) => Hp = hp;

    public void TakeDamage(int damage) => Hp -= damage;

    

}
