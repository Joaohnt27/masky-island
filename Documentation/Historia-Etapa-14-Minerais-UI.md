# Etapa 14 — Minerais ilustrados e revisão dos painéis

## Artes
Três PNGs transparentes criados com image_gen no estilo pintado à mão: triângulo turquesa, círculo âmbar e quadrado violeta. Aplicados aos ícones do inventário, seleção de cristais e encaixes, preservando as formas do puzzle. As rochas pintadas no cenário e a mecânica de mineração permanecem as existentes.

## Interface
Inventário ampliado, com fundo escurecido, três cartões claros, ilustrações maiores e estados explícitos: NÃO ENCONTRADO, GUARDADO e USADO NO ALTAR. Itens não encontrados/usados aparecem esmaecidos; o contador continua mostrando os cristais disponíveis. Botão de fechar no cabeçalho e instrução de uso no rodapé.

Mapa ampliado, com bordas douradas, área ilustrada maior, painel de destino separado e próximo objetivo destacado no rodapé. Viagem rápida continua limitada a locais visitados e o mapa continua dependendo da entrega do NPC. Ícones inferiores e atalhos preservados.

## Arquivos criados
- Assets/Art/UI/Crystals/Triangle-v1.png, Circle-v1.png e Square-v1.png, com .meta.
- Tests/UIAuthoring/AuthorMineralUI.cs.
- Tests/GameplayValidation/MineralUIValidation.cs.
- Documentation/MineralGenerationPrompts.txt: prompts das três gerações.

## Arquivos modificados
- Assets/Scripts/CrystalSymbolGraphic.cs: textura ilustrada opcional, mantendo desenho vetorial como fallback e referências existentes.
- Assets/Scripts/CrystalInventoryUI.cs: nomes/formas e estados legíveis, opacidade por disponibilidade.
- Assets/Scripts/IslandMapUI.cs: objetivo apresentado separadamente da localização atual.
- Beach, House, Island, Cave e Lighthouse: layout, imagens e referências configurados.

## Configuração e assets
Nenhuma configuração manual necessária. Nenhum asset obrigatório faltante. Artes dentro do projeto, com transparência preservada e importação Unity limitada a 512px. Nenhum pacote novo.

## Validação
- Compilação sem erros; 24 verificações aprovadas em MineralUI/ValidationResults.txt.
- Inventário vazio, coleta, três cristais, item usado e contador disponível validados.
- PNGs nos ícones de seleção e encaixe, erro sem perda do cristal e encaixe correto preservados.
- Abertura dos painéis nos cinco cenários e restrição de viagem para local não visitado validadas.
- Capturas revisadas em 1280×720: inventário cheio, mapa e encaixes. Capturas adicionais de inventário vazio/usado incluídas.
- Exceção conhecida do índice UnityEditor.Search.SearchDatabase registrada separadamente; nenhuma exceção de gameplay aceita.
- Validação focada nos painéis e no puzzle de cristais. Não foi repetido o percurso completo até os créditos.
