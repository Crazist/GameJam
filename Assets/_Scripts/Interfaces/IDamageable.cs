public interface IDamageable
{
    void ReceiveDamage(int amount);

    public void SetSpeedMultiplier(float multiplier);
    public void SetReceiveDamageMultiplier(float multiplier);

    public void SetDealDamageMultiplier(float multiplier);

    public bool IsDeath();
}
