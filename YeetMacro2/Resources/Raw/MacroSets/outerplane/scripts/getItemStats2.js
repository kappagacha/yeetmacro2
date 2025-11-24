// @tags=inventory
// @position=-20

const itemStats = {};
const itemGradePatterns = ['legendary', 'epic','superior'].map(ig => patterns.inventory.item.stat2.grade[ig]);
const itemTypePatterns = ['weapon', 'accessory', 'helmet', 'chestArmor', 'gloves', 'boots'].map(ig => patterns.inventory.item.stat2.type[ig]);

itemStats.itemGrade = macroService.PollPattern(itemGradePatterns).Path?.split('.').pop();
itemStats.itemType = macroService.PollPattern(itemTypePatterns).Path?.split('.').pop();

// TODO: primary1, primary2, secondary[1-4], secondary[1-4]Value, secondary[1-4]ValueType
// TODO: itemEffect

return itemStats;