// @tags=inventory
// @position=-20

const itemStats = {};
// TODO: superior and the other one
const itemGradePatterns = ['epic', 'legendary'].map(ig => patterns.inventory.item.stat2.grade[ig]);
const itemTypePatterns = ['weapon', 'accessory', 'helmet', 'chestArmor', 'gloves', 'boots'].map(ig => patterns.inventory.item.stat2.type[ig]);

itemStats.itemGrade = macroService.PollPattern(itemGradePatterns).Path?.split('.').pop();
itemStats.itemType = macroService.PollPattern(itemTypePatterns).Path?.split('.').pop();


return itemStats;