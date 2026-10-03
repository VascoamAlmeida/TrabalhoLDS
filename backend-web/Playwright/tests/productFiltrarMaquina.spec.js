const { test, expect } = require("@playwright/test");

const timestamp = Date.now();
const email = process.env.E2E_ADMIN_EMAIL;
const password = process.env.E2E_ADMIN_PASSWORD;

test.skip(!email || !password, "Set E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD to run administrator E2E tests.");
const name = `Playwright ${timestamp}`;
const categoriaNome = `Categoria Playwright ${timestamp}`;

async function register(page) {
    await login(page);
}

// função auxiliar de login como cliente de teste
async function login(page) {
    await page.goto("/login");

    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);

    await page.getByRole("button", { name: "Entrar" }).click();
}


async function createProduct(page) {
    await register(page);

    await expect(page.getByText("Sair")).toBeVisible();

    await page.getByRole("button", { name: "Sair" }).click(); // clicar no botao de sair

    await login(page);

    await createCategory(page);

    await page.goto("/product-list");
    await page.waitForLoadState("networkidle");

    // Clica em "Adicionar" para abrir o modal
    await page.click("text=Novo Produto");

    // Garante que o modal de criação apareceu
    const modal = page.locator("#createModal");
    await expect(modal).toBeVisible();


    const inputName = modal.locator('input[name="Name"]');
    await expect(inputName).toBeVisible();

    // Preenche campos principais
    await inputName.fill(name);

    // Categoria – escolhemos a primeira opção válida (index 1)
    await modal.locator('select[name="CategoryId"]').selectOption({ index: 1 });

    // Marca como maquina
    await modal.locator('select[name="Type"]').selectOption({ index: 1 });

    await modal.locator('input[name="Brand"]').fill("Marca Playwright");
    await modal.locator('input[name="Model"]').fill("Modelo PW");
    await modal.locator('input[name="YearManufacture"]').fill("2024");
    await modal.locator('input[name="Price"]').fill("999");
    await modal.locator('input[name="Quantity"]').fill("3");

    await modal.locator('textarea[name="Description"]').fill("Criado automaticamente por teste Playwright.");
    await modal.locator('input[name="Image"]').fill("exemplo teste");

    // Marca o produto como ativo
    await modal.getByRole("checkbox", { name: "Produto Ativo" }).check();

    await page.click("text=Guardar");

    // Tempo para dar reload
    await page.waitForTimeout(3500);
    await page.waitForLoadState("networkidle");

    // Verifica que o novo produto aparece na lista
    const card = page.locator(".product-card", { hasText: name }).first();
    await expect(card).toBeVisible();
}

async function createCategory(page) {
    await page.goto("/category-list");
    await page.waitForLoadState("networkidle");

    // 1. Clica em "Adicionar" para abrir o modal
    await page.click("text=Adicionar");

    // 2. Garante que o modal de criação apareceu
    const modal = page.locator("#createModal");
    await expect(modal).toBeVisible();

    const inputName = modal.locator('input[name="Name"]');
    const inputDesc = modal.locator('input[name="Description"]');
    await expect(inputName).toBeVisible();

    // 4. Preenche os campos com um nome único
    const timestamp = Date.now();
    const categoriaDescription = "Descrição Playwright";
    await inputName.fill(categoriaNome);
    await inputDesc.fill(categoriaDescription);

    // 5. Clica em Guardar (botão com id CreateSaveButton)
    await page.click("#CreateSaveButton");

    await expect(page.getByText("Fornecedor")).toBeVisible();
}

test("Catálogo - Filtrar por maquina", async ({ page }) => {
    await createProduct(page);

    await page.goto("/product-list");
    await page.waitForLoadState("networkidle");

    // 2. Garante que a lista de produtos existe
    const cards = page.locator(".product-card");

    // 3. Conta quantos cards existem
    const count = await cards.count();

    // 4. Verifica se há pelo menos 1 produto
    await expect(count).toBeGreaterThan(0);

    await page.locator("#filterStatus").selectOption({ index: 1 });
    await page.waitForLoadState('networkidle');

    // pega em todos os cards visíveis
    const cardsFiltrados = page.locator('.product-card');

    // 3. Conta quantos cards existem
    const count2 = await cardsFiltrados.count();

    // garante que há pelo menos 1
    await expect(cardsFiltrados).toHaveCount(count2);

    const badges = cardsFiltrados.locator('.status-badge.machine');
    const badgeCount = await badges.count();
    //expect(badgeCount).toBe(count2); 

    for (let i = 0; i < badgeCount; i++) {
        await expect(badges.nth(i)).toHaveText("M\u00E1quina");
    }
});