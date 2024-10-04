using System;
using Microsoft.Xna.Framework.Content;

namespace RolePlayingGameData;

public class EquipmentReader : IContentTypeReaderDelegate<Equipment>
{
    private readonly IContentTypeReaderDelegate<Gear> gearReader = new GearReader();

    public Equipment Read(ContentReader input,
        Equipment existingInstance)
    {
        Equipment equipment = existingInstance;

        if (equipment == null)
        {
            throw new ArgumentException(
                "Unable to create new Equipment objects.");
        }

        // read the gear settings
        gearReader.Read(input, equipment);

        // read the equipment settings
        equipment.OwnerBuffStatistics = input.ReadObject<StatisticsValue>();

        return equipment;
    }
}
