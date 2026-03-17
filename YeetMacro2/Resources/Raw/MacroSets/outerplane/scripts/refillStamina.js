// @raw-script
// @tags=favorites,weeklies
function refillStamina(targetStamina) {

	if (!targetStamina) targetStamina = settings.refillStamina.targetStamina.Value;

	goToLobby();

	let currentStamina = getCurrentStaminaValue();
	logger.info(`refillStamina to ${targetStamina}. current stamina is ${currentStamina}`);
	if (currentStamina >= targetStamina) {
		return;
	}

	while (macroService.IsRunning && currentStamina < targetStamina) {
		const diffStamina = targetStamina - currentStamina;
		const chestToStaminaConversionRate = 6;
		const targetNumChests = Math.ceil(diffStamina / chestToStaminaConversionRate);
		macroService.PollPattern(patterns.tabs.inventory, { DoClick: true, PredicatePattern: patterns.titles.inventory });
		macroService.PollPattern(patterns.inventory.consumables, { DoClick: true, PredicatePattern: patterns.inventory.consumables.use });
		const chestResult = macroService.FindPattern(patterns.inventory.consumables.chest, { Limit: 10 });
		if (!chestResult.IsSuccess) return new Error('Could not find chest pattern');

		// Sort points by Y (top first), then X (left first)
		const sortedPoints = [...chestResult.Points].sort((a, b) => {
			const yDiff = a.Y - b.Y;
			if (Math.abs(yDiff) > 5) return yDiff;
			return a.X - b.X;
		});
		const chestPoint = sortedPoints.at(settings.refillStamina.chestIndex.Value ?? -1);
		macroService.PollPoint(chestPoint, { PredicatePattern: patterns.inventory.consumables.chest.adventurer });
		macroService.PollPattern(patterns.inventory.consumables.use, { DoClick: true, PredicatePattern: patterns.inventory.consumables.use.ok });
		macroService.PollPattern(patterns.inventory.consumables.use.min, { DoClick: true, PredicatePattern: patterns.inventory.consumables.use.sliderMin });

		let currentAmount = macroService.FindText(patterns.inventory.consumables.use.currentAmount);
		while (macroService.IsRunning && currentAmount <= targetNumChests) {
			macroService.ClickPattern(patterns.inventory.consumables.use.plus);
			sleep(100);
			currentAmount = macroService.FindText(patterns.inventory.consumables.use.currentAmount);
		}
		macroService.PollPattern(patterns.inventory.consumables.use.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.inventory });
		sleep(200);
		currentStamina = getCurrentStaminaValue();
	}
}