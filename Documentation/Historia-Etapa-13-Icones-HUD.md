# Etapa 13 — Ícones de mapa e inventário

## Resultado
Dois PNGs transparentes no estilo pintado à mão do jogo: mapa em pergaminho e bolsa de couro azul-petróleo. Mapa no canto inferior esquerdo; inventário acima, com margem e intervalo. Pequenas marcas M e I lembram os atalhos. Os dois ícones são botões e usam os painéis já existentes.

O mapa mantém a entrega obrigatória pelo NPC. O inventário mantém a liberação no início da missão. Os ícones ficam ocultos durante os painéis e a pausa; não reintroduzem a barra inferior removida. O painel de feedback de mineração da caverna foi centralizado na parte inferior para evitar sobreposição.

## Arquivos criados
- Assets/Art/UI/Icons/Map-v1.png e Inventory-v1.png, com metadados: imagens criadas pela ferramenta integrada image_gen, fundo transparente.
- Tests/UIAuthoring/AuthorHudIcons.cs: integração incremental nas cinco cenas.
- Tests/GameplayValidation/HudIconsValidation.cs: verificação dos botões e atalhos.
- Documentation/HudIconGenerationPrompts.txt: prompts finais usados nas duas gerações.

## Arquivos modificados
- Assets/Scripts/CrystalInventoryUI.cs: referência ao botão de abertura e controle de disponibilidade/visibilidade.
- Assets/Scenes/Beach.unity, House.unity, Island.unity, Cave.unity e Lighthouse.unity: ícones, posições, sprites e callbacks configurados.

## Configuração e assets
Nenhuma configuração manual necessária. Nenhum asset obrigatório faltante. PNGs originais preservados; importação Unity como Sprite com transparência e resolução máxima de 256 para o HUD. Fundos transparentes verificados nos arquivos.

## Validação
- Compilação sem erros e 46 verificações aprovadas: HudIcons/ValidationResults.txt.
- Cinco cenários: sprites atribuídos, posição inferior esquerda, bolsa acima do mapa, raycast no centro dos botões, abertura dos painéis e preservação da barra inferior removida.
- Atalhos I/M, bloqueio anterior à missão/entrega do mapa e ocultação na pausa validados.
- Feedback de mineração sem sobreposição com os ícones.
- Capturas finais em 1280×720 revisadas: HudIcons/hud-icons-house.png e hud-icons-cave.png.
- O teste de raycast foi ajustado ao centro dos novos botões e sincronizado com a renderização do Canvas na execução batch.
- Exceção conhecida de UnityEditor.Search.SearchDatabase registrada separadamente; nenhuma exceção de gameplay aceita.
- Validação focada na UI; o percurso completo até os créditos não foi repetido.
