// @raw-script
// @tags=favorites
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

		const lastChestPoint = chestResult.Points.reduce((lastChest, p) => {
			const yDiff = p.Y - lastChest.Y;

			// If p is significantly lower (more than 5 pixels), choose p
			if (yDiff > 5) {
				return p;
			}
			// If p is significantly higher (more than 5 pixels), keep lastChest
			if (yDiff < -5) {
				return lastChest;
			}
			// Y values are within 5 pixels - find rightmost
			if (p.X >= lastChest.X) {
				return p;
			}
			return lastChest;
		});
		macroService.PollPoint(lastChestPoint, { PredicatePattern: patterns.inventory.consumables.chest.adventurer });
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