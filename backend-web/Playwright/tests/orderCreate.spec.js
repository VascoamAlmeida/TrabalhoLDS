const { test, expect } = require("@playwright/test");

const timestamp = Date.now();
const email = process.env.E2E_ADMIN_EMAIL;
const password = process.env.E2E_ADMIN_PASSWORD;

test.skip(!email || !password, "Set E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD to run administrator E2E tests.");

const emailClient = `playwrightC${timestamp}@email.com`;
const passwordClient = "Teste123!C";

const name = `Playwright ${timestamp}`;
const categoriaNome = `Categoria Playwright ${timestamp}`;
const nifClient = generateNif();
const guidStock = generateGuid();


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

async function registerAndLoginClient(page) {

    await page.goto("/register");

    /// Preenche os campos de email e pass
    await page.fill('input[name="Email"]', emailClient);
    await page.fill('input[name="Password"]', passwordClient);
    await page.getByRole("button", { name: "Registar" }).click();
    await page.getByRole("button", { name: "Sair" }).click();

    await page.goto("/login");

    await page.fill('input[name="email"]', emailClient);
    await page.fill('input[name="password"]', passwordClient);

    await page.getByRole("button", { name: "Entrar" }).click();
}
function generateNif() {
    // número entre 100000000 e 999999999
    return String(Math.floor(100000000 + Math.random() * 900000000));
}

function generateGuid() {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, c => {
        const r = (Math.random() * 16) | 0;
        const v = c === "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
}

async function editarPerfil(page) {
    // deve redirecionar para /profiled
    await page.waitForURL("**/profile");

    // Preenche alguns campos do Client
    await page.fill('input[name="Client.FirstName"]', "Playwright Teste");
    await page.fill('input[name="Client.LastName"]', "Playwright");
    await page.fill('input[name="Client.Address"]', "Rua dos Testes 123");
    await page.fill('input[name="Client.PostalCode"]', "1000-001");
    await page.fill('input[name="Client.PhoneNumber"]', "911512461");
    await page.fill('input[name="Client.NIF"]', nifClient);

    // Submete o formulário de perfil
    await page.getByRole("button", { name: "Guardar Altera\u00E7\u00F5es" }).click();

    await page.waitForURL("**/profile"); // há redirect

    await page.waitForLoadState("networkidle");

    // Verifica se o primeiro nome ficou com o valor que introduzimos
    await expect(page.locator('input[name="Client.FirstName"]')).toHaveValue("Playwright Teste");
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

async function createProduct(page) {
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

async function addStock(page) {
    await page.goto("/stock-list");
    await page.waitForLoadState("networkidle");

    // Clica em "Adicionar" para abrir o modal 
    await page.click("text=Novo Item");

    // Garante que o modal de criação apareceu 
    const modal = page.locator("#createModal");
    await expect(modal).toBeVisible();

    await modal.locator('input[name="Ref"]').fill(guidStock);

    await modal.locator('select[name="ProductId"]').selectOption({ label: name });
    await modal.locator('select[name="StockStatus"]').selectOption({ index: 1 });

    await page.click("text=Guardar");

    await page.waitForLoadState("networkidle");
}


test("Order – Criar Order", async ({ page }) => {
    await register(page);
    await page.getByRole("button", { name: "Sair" }).click();
    await login(page);

    await createCategory(page);
    await createProduct(page);

    await addStock(page);

    await page.getByRole("button", { name: "Sair" }).click();

    await registerAndLoginClient(page);

    await editarPerfil(page);

    await page.goto("/product-list");
    await page.waitForLoadState("networkidle");

    await page.fill("#filterName", name);

    await page.goto("/product-list");
    await page.waitForLoadState("networkidle");

    const card = page.locator(".product-card", { hasText: name }).first();
    await expect(card).toBeVisible();

    const fazerPedidoBtn = card.getByRole("link", { name: "Fazer pedido" });
    await Promise.all([
        page.waitForURL("**/order-create/**"),
        fazerPedidoBtn.click()
    ]);


    // Estamos na página Criar Pedido
    await expect(
        page.getByRole("heading", { name: "Criar Pedido" })
    ).toBeVisible();

    // 5) Mudar "Tipo de Pedido" para Aluguer
    await page.selectOption("#OrderType", "0");

    // 6) Confirmar Pedido -> vai para Novo Aluguer
    await Promise.all([
        page.getByRole("button", { name: "Confirmar Pedido" }).click(),
    ]);

    await page.click("text=Sim");

    await expect(
        page.getByRole("heading", { name: "Finalizar Compra" })
    ).toBeVisible();

    // Tempo para dar reload
    await page.waitForTimeout(2500);
    await page.waitForLoadState("networkidle");

    await expect(
        page.getByRole("heading", { name: "Finalizar Compra" })
    ).toBeVisible();

    await page.getByRole("button", { name: "Finalizar Compra" }).click();

    await page.click("text=Sim, confirmar!");

    // Tempo para dar reload
    await page.waitForTimeout(2500);
    await page.waitForLoadState("networkidle");

    await expect(page.getByRole("heading", { name: /Hist.*de Pedidos/ })
    ).toBeVisible();

});