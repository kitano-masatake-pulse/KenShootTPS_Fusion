public struct OptionData
{
    public float mouseSensitivity;
    public bool invertY;
    public bool invertX;
    public float masterVolume;
    public float systemVolume;
    public float bgmVolume;
    public float weaponVolume;
    public float actionVolume;
    public OptionData(float mouseSensitivity, bool invertY, bool invertX,
                       float masterVolume, float systemVolume, float bgmVolume,
                       float weaponVolume, float actionVolume)
    {
        this.mouseSensitivity = mouseSensitivity;
        this.invertY = invertY;
        this.invertX = invertX;
        this.masterVolume = masterVolume;
        this.systemVolume = systemVolume;
        this.bgmVolume = bgmVolume;
        this.weaponVolume = weaponVolume;
        this.actionVolume = actionVolume;
    }
    public static OptionData Default()
    {
        return new OptionData(
            mouseSensitivity: 0f,
            invertY: false,
            invertX: false,
            masterVolume: 1.0f,
            systemVolume: 1.0f,
            bgmVolume: 1.0f,
            weaponVolume: 1.0f,
            actionVolume: 1.0f
        );
    }

}
