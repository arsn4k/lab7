const API = (window.ITOPS_API_URL || "").replace(/\/$/, "");
const STATUSES = ["Healthy", "Degraded", "Down", "Maintenance"];
const $ = (id) => document.getElementById(id);

async function request(path, options = {}) {
  const response = await fetch(API + path, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    throw new Error(body.message || body.title || `HTTP ${response.status}`);
  }
  return response.status === 204 ? null : response.json();
}

function cell(text) {
  const td = document.createElement("td");
  td.textContent = text ?? "—"; // textContent: дані з API не інтерпретуються як HTML
  return td;
}

async function checkHealth() {
  const badge = $("health");
  try {
    const health = await request("/health");
    badge.textContent = `API: ${health.status}`;
    badge.className = "badge " + (health.status === "Healthy" ? "ok" : "bad");
  } catch (e) {
    badge.textContent = "API недоступний";
    badge.className = "badge bad";
  }
}

async function loadServers() {
  const tbody = $("servers");
  tbody.replaceChildren();
  const servers = await request("/api/servers");
  for (const s of servers) {
    const metrics = await request(`/api/servers/${s.id}/metrics?take=1`);
    const last = metrics[0];
    const tr = document.createElement("tr");
    tr.append(
      cell(s.hostname), cell(s.ip_address), cell(s.environment),
      cell(`${s.cpu_alert_threshold} %`),
      statusCell(s),
      cell(last ? `CPU ${last.cpu_usage}% · RAM ${last.memory_usage}%` : null),
      actionsCell(s),
    );
    tbody.append(tr);
  }
}

function statusCell(server) {
  const td = document.createElement("td");
  const select = document.createElement("select");
  STATUSES.forEach((name, value) => select.add(new Option(name, value, false, value === server.status)));
  select.onchange = () => run(() => request(`/api/servers/${server.id}`, {
    method: "PUT",
    body: JSON.stringify({
      hostname: server.hostname, ip_address: server.ip_address,
      environment: server.environment, cpu_alert_threshold: server.cpu_alert_threshold,
      status: Number(select.value),
    }),
  }), `Стан ${server.hostname} змінено`);
  td.append(select);
  return td;
}

function actionsCell(server) {
  const td = document.createElement("td");
  const metric = document.createElement("button");
  metric.textContent = "+ метрика";
  metric.className = "link";
  metric.onclick = () => run(() => request(`/api/servers/${server.id}/metrics`, {
    method: "POST",
    body: JSON.stringify({
      cpu_usage: +(Math.random() * 100).toFixed(1),
      memory_usage: +(Math.random() * 100).toFixed(1),
    }),
  }), `Додано вимірювання для ${server.hostname}`);

  const remove = document.createElement("button");
  remove.textContent = "видалити";
  remove.className = "link danger";
  remove.onclick = () => confirm(`Видалити ${server.hostname}?`) &&
    run(() => request(`/api/servers/${server.id}`, { method: "DELETE" }), `${server.hostname} видалено`);

  td.append(metric, remove);
  return td;
}

async function run(action, successText) {
  try {
    await action();
    $("message").textContent = successText;
    await loadServers();
  } catch (e) {
    $("message").textContent = "Помилка: " + e.message;
  }
}

$("create-form").onsubmit = (event) => {
  event.preventDefault();
  const data = Object.fromEntries(new FormData(event.target));
  data.cpu_alert_threshold = Number(data.cpu_alert_threshold);
  run(() => request("/api/servers", { method: "POST", body: JSON.stringify(data) }),
      `Сервер ${data.hostname} створено`).then(() => event.target.reset());
};
$("reload").onclick = () => run(() => Promise.resolve(), "Список оновлено");

$("api").textContent = API;
checkHealth();
run(() => Promise.resolve(), "");
