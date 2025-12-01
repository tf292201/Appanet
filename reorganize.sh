#!/bin/bash

echo "🚀 Starting Appanet project reorganization..."

# Create new directory structure
echo "📁 Creating new folders..."
mkdir -p Scenes/Combat
mkdir -p Scenes/UI
mkdir -p Scenes/Entities
mkdir -p Scripts/Combat
mkdir -p Scripts/UI
mkdir -p Scripts/Entities
mkdir -p Scripts/Models/Characters
mkdir -p Scripts/Models/Items
mkdir -p Scripts/Models/Combat
mkdir -p Scripts/Models/SpecialAbilities
mkdir -p Assets/Fonts
mkdir -p Assets/Icons

echo "✅ Folders created!"

# Move Scene files
echo "📦 Moving scene files..."
mv CombatTest.tscn Scenes/Combat/
mv AttackTimingMinigame.tscn Scenes/Combat/
mv InventoryScene.tscn Scenes/UI/
mv Opening.tscn Scenes/UI/
mv TestScene.tscn Scenes/UI/
mv Firefly.tscn Scenes/Entities/

echo "✅ Scenes moved!"

# Move Combat scripts
echo "📜 Moving combat scripts..."
mv CombatTestController.cs Scripts/Combat/
mv CombatTestController.cs.uid Scripts/Combat/

echo "✅ Combat scripts moved!"

# Move UI scripts
echo "📜 Moving UI scripts..."
mv InventorySceneController.cs Scripts/UI/
mv Opening.cs Scripts/UI/

echo "✅ UI scripts moved!"

# Move Entity scripts
echo "📜 Moving entity scripts..."
mv Firefly.cs Scripts/Entities/

echo "✅ Entity scripts moved!"

# Move Model files - Characters
echo "📜 Moving character models..."
mv Models/Character.cs Scripts/Models/Characters/
mv Models/Player.cs Scripts/Models/Characters/
mv Models/Ally.cs Scripts/Models/Characters/
mv Models/Enemy.cs Scripts/Models/Characters/

# Move Model files - Items
echo "📜 Moving item models..."
mv Models/Item.cs Scripts/Models/Items/
mv Models/Weapon.cs Scripts/Models/Items/
mv Models/Armor.cs Scripts/Models/Items/
mv Models/Consumable.cs Scripts/Models/Items/

# Move Model files - Combat
echo "📜 Moving combat models..."
mv Models/CombatEnums.cs Scripts/Models/Combat/
mv Models/AttackResults.cs Scripts/Models/Combat/
mv Models/DamageResistance.cs Scripts/Models/Combat/
mv Models/StatusEffectInstance.cs Scripts/Models/Combat/
mv Models/CombatParticipant.cs Scripts/Models/Combat/
mv Models/CombatState.cs Scripts/Models/Combat/
mv Models/AttackTimingMinigame.cs Scripts/Models/Combat/

# Move Model files - Special Abilities
echo "📜 Moving special abilities..."
mv Models/SpecialAbilities/SpecialAbility.cs Scripts/Models/SpecialAbilities/
mv Models/SpecialAbilities/StreetLightsComingOn.cs Scripts/Models/SpecialAbilities/
mv Models/SpecialAbilities/StaticBurst.cs Scripts/Models/SpecialAbilities/
mv Models/SpecialAbilities/HaintWind.cs Scripts/Models/SpecialAbilities/
mv Models/SpecialAbilities/MKUltraMemoryScramble.cs Scripts/Models/SpecialAbilities/

# Move remaining model files
echo "📜 Moving remaining model files..."
mv Models/Inventory.cs Scripts/Models/

# Move font files
echo "🔤 Moving fonts..."
mv fonts/* Assets/Fonts/
rmdir fonts

# e icons (if you want - this is optional since you have many)
echo "🎨 Moving icons..."
if [ -d "icons" ]; then
    mv icons/* Assets/Icons/
    rmdir icons
fi

# Clean up old Models directory
echo "🧹 Cleaning up old Models folder..."
rmdir Models/SpecialAbilities
rmdir Models

# Keep TestButton.cs in Scripts root for now
echo "📜 TestButton.cs stays in Scripts/"

echo ""
echo "✅ ✅ ✅ REORGANIZATION COMPLETE! ✅ ✅ ✅"
echo ""
