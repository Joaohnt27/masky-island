"""Gera a documentação acadêmica e arquivos de apoio a partir do projeto local.
Executar com Python + reportlab + Pillow + pypdfium2. Não altera cenas.
"""
from pathlib import Path
import shutil
from xml.sax.saxutils import escape
from reportlab.pdfgen import canvas
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Image, Table, TableStyle, PageBreak
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.colors import HexColor, white
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from PIL import Image as PILImage, ImageOps
import pypdfium2 as pdfium

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'output' / 'Entrega'
OUT.mkdir(parents=True, exist_ok=True)
PUB = OUT / 'Publicacao'
PUB.mkdir(exist_ok=True)
pdfmetrics.registerFont(TTFont('Body', 'C:/Windows/Fonts/arial.ttf'))
pdfmetrics.registerFont(TTFont('Bold', 'C:/Windows/Fonts/arialbd.ttf'))
pdfmetrics.registerFont(TTFont('Title', 'C:/Windows/Fonts/georgiab.ttf'))
pdfmetrics.registerFontFamily('Body', normal='Body', bold='Bold', italic='Body', boldItalic='Bold')
TEAL = HexColor('#214C50'); GOLD = HexColor('#AD792C'); INK = HexColor('#25373A')
styles = {
 'body': ParagraphStyle('body', fontName='Body', fontSize=10.3, leading=15.5, textColor=INK, spaceAfter=10),
 'h1': ParagraphStyle('h1', fontName='Title', fontSize=23, leading=29, textColor=TEAL, spaceAfter=18),
 'h2': ParagraphStyle('h2', fontName='Bold', fontSize=12.5, leading=18, textColor=GOLD, spaceBefore=9, spaceAfter=7),
 'caption': ParagraphStyle('caption', fontName='Body', fontSize=8.5, leading=12, textColor=HexColor('#53676A'), spaceAfter=12),
 'cell': ParagraphStyle('cell', fontName='Body', fontSize=9.2, leading=13, textColor=INK),
 'cover': ParagraphStyle('cover', fontName='Title', fontSize=38, leading=44, textColor=TEAL, alignment=TA_CENTER, spaceAfter=12),
 'sub': ParagraphStyle('sub', fontName='Body', fontSize=13, leading=20, alignment=TA_CENTER, textColor=INK, spaceAfter=12),
}
flow=[]
def p(text, style='body'): flow.append(Paragraph(text, styles[style]))
def heading(number, text): p(number+' / '+text, 'h1')
def photo(relative, caption, width=487):
 path=ROOT/relative
 with PILImage.open(path) as im: w,h=im.size
 flow.append(Image(str(path), width=width, height=width*h/w))
 flow.append(Spacer(1,7)); p(caption,'caption')
def table(rows,widths):
 cells=[[Paragraph(escape(str(c)),styles['cell']) for c in r] for r in rows]
 t=Table(cells,colWidths=widths,hAlign='LEFT')
 t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),HexColor('#DDE7E3')),('VALIGN',(0,0),(-1,-1),'TOP'),('LEFTPADDING',(0,0),(-1,-1),9),('RIGHTPADDING',(0,0),(-1,-1),9),('TOPPADDING',(0,0),(-1,-1),8),('BOTTOMPADDING',(0,0),(-1,-1),8),('LINEBELOW',(0,0),(-1,-1),.4,HexColor('#CDD6D2'))]))
 flow.append(t);flow.append(Spacer(1,12))
def page(): flow.append(PageBreak())
def chrome(c,doc):
 w,h=A4;c.saveState();c.setFillColor(HexColor('#FBF9F3'));c.rect(0,0,w,h,fill=1,stroke=0)
 if doc.page>1:
  c.setFont('Bold',8);c.setFillColor(TEAL);c.drawString(54,h-33,'MASKY ISLAND  |  DOCUMENTAÇÃO DO PROJETO')
 c.setStrokeColor(GOLD);c.setLineWidth(.6);c.line(54,40,w-54,40)
 c.setFont('Body',8);c.setFillColor(INK);c.drawString(54,27,'UNAERP · Engenharia de Software · Novembro de 2026');c.drawRightString(w-54,27,str(doc.page));c.restoreState()

p('MASKY ISLAND','cover')
p('Uma ilha, três lembranças e um farol apagado.','sub')
photo('Assets/Art/Backgrounds/Menu/IslandBeforeStorm-v1.png','Arte do menu principal utilizada no projeto. Direção visual: aventura ilustrada em uma ilha costeira.')
p('Projeto Prático: Desenvolvimento e Publicação de Jogo Digital','h2')
p('Documentação formal e Game Design Document resumido. Versão acadêmica para Windows x64.')
p('<b>João Henrique Nazar Tavares</b> · RA 839463<br/><b>Rafael Mele Porto</b> · RA 840897')
p('<b>Professor:</b> Thiago Garcia Cajuela<br/><b>Instituição:</b> Universidade de Ribeirão Preto (UNAERP)<br/><b>Curso:</b> Engenharia de Software · 8ª etapa<br/><b>Data do documento:</b> 23 de novembro de 2026')
page()
heading('01','Conceito e motivação')
p('Masky Island é uma aventura narrativa para um jogador, com exploração, diálogos, coleta de itens e puzzles. O projeto combina cenários ilustrados e personagens em sprites com posicionamento e interação em um ambiente 3D, caracterizando uma apresentação 2.5D.')
table([['Aspecto','Definição'],['Gênero','Aventura narrativa, exploração e puzzles'],['Público-alvo','Jogadores que apreciam mistério, desafios de observação e experiências sem combate'],['Plataforma e idioma','Windows x64; português brasileiro'],['Objetivo principal','Recuperar três lembranças, preparar a chave, destrancar o farol e restaurar sua luz'],['Proposta visual','Ilha costeira, formas desenhadas, tons quentes de papel e luz, contraste com o azul do mar']],[125,362])
p('Sinopse','h2')
p('Depois de enfrentar uma tempestade, o protagonista chega à praia sem recordar sua identidade. Um diário vazio acompanha seus primeiros passos. Na casa, um aviso o leva ao centro da ilha, onde encontra um estranho que orienta sua exploração. Pistas e objetos despertam lembranças de sua ligação com o farol e de seu retorno para restaurá-lo.')
p('Motivação do jogador','h2')
p('O mistério inicial dá um motivo imediato para explorar: descobrir o que aconteceu e encontrar ajuda. A progressão transforma essa dúvida em uma responsabilidade concreta. O personagem reconhece sua história como guardião do farol e conclui sua jornada ao devolver a luz à ilha.')
p('Intenção de design','h2')
p('A aventura prioriza compreensão e descoberta. O diário registra objetivos, o NPC fornece orientação e os cenários apresentam pistas visuais. Não há moedas, combate ou limite global de tempo. Os desafios de precisão aparecem em momentos específicos, conectados a ações materiais da história.')
page()
heading('02','Estrutura da aventura')
table([['Etapa','Ação do jogador e consequência'],['Praia','Assistir à introdução, explorar e coletar o diário. A casa se torna o primeiro destino.'],['Casa e centro','Ler o aviso da casa, encontrar o NPC e concluir o diálogo. Receber o mapa e descobrir onde buscar a mochila e a picareta.'],['Casa','Coletar a mochila e resolver os símbolos celestes para recuperar a lembrança de sua ligação com o farol.'],['Caverna','Coletar a picareta, minerar os três cristais e encaixá-los no altar. Recuperar a lembrança da tempestade e abrir o esconderijo.'],['Volta à casa','Levar os materiais à bancada, fabricar e recolher a chave. A lembrança do retorno completa a motivação da jornada.'],['Exterior do farol','Ler a inscrição, usar a chave e decifrar a combinação do cadeado para liberar a entrada.'],['Interior e encerramento','Realizar os três ajustes do mecanismo. A luz é restaurada; o vídeo final conduz aos créditos.']],[120,367])
p('Progressão sem perder a liberdade','h2')
p('O mundo permite revisitar os cenários. Os requisitos dos objetos garantem a ordem lógica da história, sem obrigar a criação de áreas completamente fechadas. A viagem rápida só alcança lugares visitados e o destino do farol é seu exterior, preservando o enigma da entrada.')
p('O nome escolhido identifica o jogador nas falas em que isso é adequado. A amnésia permanece como elemento narrativo: não é necessário apagar a personalização para comunicar a perda das lembranças.')
page()
heading('03','Mecânicas e regras')
p('Mineração e inventário','h2')
p('A mineração exige os requisitos da missão, a mochila, a picareta e a aproximação de uma rocha válida. Na configuração atual da caverna, cada tentativa dura quatro segundos e tem 35% de chance de descoberta. Uma tentativa pode não encontrar nada. O progresso, os golpes e o som comunicam a atividade; outra mineração não pode começar simultaneamente.')
p('Os cristais encontrados entram no inventário. Quando as três formas são encaixadas corretamente no altar, o estado permanece registrado ao sair e retornar à caverna. A recompensa abre um esconderijo com materiais para a chave, ligando mineração, puzzle e crafting.')
p('Fabricação e cadeado','h2')
p('Na bancada, o jogador prepara a receita e executa três ações de precisão, confirmando quando o indicador está dentro da faixa-alvo. A receita é concluída ao recolher a chave; erros não destroem os materiais. No farol, o jogador precisa interpretar a inscrição, usar a chave fabricada e acertar a combinação. Uma tentativa incorreta não consome a chave.')
p('Restauração da luz','h2')
p('O desafio final reaproveita a linguagem da bancada: alinhar lentes, regular engrenagens e acender a luz. Cada etapa tem uma janela local de oito segundos. A faixa de acerto diminui e o indicador acelera nas etapas seguintes. Erros permitem nova tentativa e preservam ajustes já feitos enquanto a atividade continua. Cancelar encerra a tentativa; não há derrota global por tempo.')
photo('Documentation/CraftingGameplay/crafting-workbench.png','Bancada de fabricação: feedback visual e ação de precisão integrados à arte da interface.',width=420)
page()
heading('04','Controles e interface')
table([['Entrada','Função'],['W, A, S, D','Mover o personagem.'],['Clique esquerdo','Selecionar objetos e interagir; o personagem se aproxima do ponto de interação.'],['I','Abrir ou fechar a mochila depois de coletá-la.'],['M','Abrir o mapa após recebê-lo do NPC.'],['J','Abrir o diário após coletá-lo na praia.'],['Esc','Fechar interfaces ou abrir a pausa, conforme o contexto.'],['Espaço / botão do painel','Confirmar a ação de precisão na fabricação e no farol.'],['Botões de diálogo e vídeo','Avançar as falas e pular animações quando indicado na tela.']],[145,342])
p('Interface integrada à história','h2')
p('Diário, mochila e mapa usam representações ilustradas e são liberados por acontecimentos da aventura. O diário registra e risca tarefas; a mochila apresenta os itens; o mapa ajuda a localizar e revisitar áreas conhecidas. Os diálogos combinam retratos, papel ilustrado e as fontes do projeto, DynaPuff e Kablammo.')
p('Pausa, configurações e persistência','h2')
p('O menu permite ajustar opções de áudio e vídeo e consultar os controles. O progresso da campanha é mantido durante as trocas de cena na mesma sessão. Ao fechar o jogo ou iniciar uma nova partida, esse progresso não é recuperado. Nome e configurações locais são armazenados separadamente; não há sistema completo de save/load de campanha.')
photo('Documentation/LighthouseQte/lighthouse-qte-playing.png','Mecanismo do farol: etapas, faixa de acerto e indicação explícita da ação.',width=250)
page()
heading('05','Arquitetura e tecnologias')
p('<b>Motor:</b> Unity 6000.6.0f1. <b>Linguagem:</b> C#. <b>Renderização:</b> Universal Render Pipeline 17.6.0. <b>Entrada:</b> New Input System 1.20.0. <b>Interface:</b> uGUI/TextMeshPro. <b>Mídia:</b> AudioSource e VideoPlayer da Unity. As dependências reproduzíveis constam em Packages/manifest.json e packages-lock.json.')
table([['Componente','Responsabilidade'],['GameManager','Fonte central de progresso: inventário, lembranças, locais visitados e desbloqueios; persiste com DontDestroyOnLoad.'],['PlayerClickMovement / Interactable','Entrada, aproximação, interação e bloqueios de movimento durante atividades.'],['DialogueController / NPCInteraction','Falas, retratos e início da missão ao concluir o diálogo.'],['MiningSpot / MiningUI','Requisitos, coroutine de mineração, descoberta e feedback.'],['KeyCraftingUI / LighthouseGate','Receita da chave, ações de precisão, pista e combinação do cadeado.'],['LighthouseInteraction / EndingCutscene','Ajustes finais, conclusão, vídeo e passagem aos créditos.'],['QuestJournalUI / IslandMapUI / CrystalInventoryUI','Representações do estado do jogador, sem substituir o gerenciador central.'],['SceneFade / SettingsManager','Transições entre cenas e preferências locais.']],[185,302])
p('Organização e ciclo de desenvolvimento','h2')
p('Assets reúne cenas, scripts, arte e recursos de execução; Assets/Editor contém ferramentas exclusivas do Editor. Tests e Documentation registram autoria, validações e evolução. O projeto já possuía configuração de Unity Version Control. A entrega GitHub importa a versão atual com commits reais, sem reconstruir artificialmente um histórico retroativo.')
p('Não foram adicionados frameworks externos de gameplay. Fontes e pacotes mantêm seus avisos de licença. Efeitos cartunescos são sintetizados por uma ferramenta do projeto; parte da arte foi produzida com assistência de geração de imagens. Os vídeos foram fornecidos pela equipe.')
page()
heading('06','Desafios e conclusão')
p('Estado consistente entre cenários','h2')
p('Um problema observado foi o desaparecimento visual dos cristais após fechar o puzzle ou mudar de área. A solução separou a representação do altar do estado persistente dos encaixes no GameManager, permitindo reconstruir corretamente os objetos quando a cena retorna.')
p('Interações concorrentes e movimento','h2')
p('Mineração, diálogos e puzzles podem disputar o controle do personagem. Bloqueios por proprietário impedem que o fechamento de um painel libere indevidamente o controle ainda usado por outra atividade. As condições de interação também evitam ações duplicadas e progressão fora de ordem.')
p('Coerência visual e orientação','h2')
p('A interface evoluiu de painéis genéricos para diário, mochila, mapa e papéis ilustrados. A orientação foi integrada aos objetos e às falas. O crafting e o desafio do farol compartilham a mesma lógica visual de precisão, reduzindo a necessidade de aprender um controle diferente no clímax.')
p('Validação e distribuição','h2')
p('O projeto inclui relatórios de testes direcionados de menu, mineração, inventário, crafting, cadeado e encerramento. Eles verificam requisitos, transições, persistência e feedback em cópias isoladas. Alguns testes preparam o estado anterior do jogador; por isso, não equivalem a uma partida manual completa. A entrega inclui um checklist de aceitação para a build distribuída.')
report=OUT/'Windows'/'build-report.txt'
if report.exists() and 'Result: Succeeded' in report.read_text(encoding='utf-8-sig'):
 p('A compilação Windows x64 da entrega foi concluída com sucesso. O relatório técnico acompanha a build. A publicação e o teste de download público são registrados separadamente no status de entrega; compilar com sucesso, sozinho, não comprova a hospedagem.')
else:
 p('A versão de distribuição prevista é Windows x64 no Itch.io. O resultado da compilação, a verificação de execução e os links públicos devem ser conferidos no registro de entrega que acompanha este documento.')
p('Conclusão','h2')
p('Masky Island articula narrativa, exploração, coleta e puzzles em uma jornada com começo, desenvolvimento e encerramento. A principal aprendizagem foi manter as regras centralizadas enquanto cenários e interfaces evoluíam de forma incremental. Como continuidade, destacam-se save de campanha, testes com novos jogadores, acessibilidade e balanceamento do tempo de mineração.')
p('Referências do projeto','h2')
p('Código-fonte: https://github.com/Joaohnt27/masky-island<br/>Evidências técnicas: Documentation e Tests no repositório.<br/>Créditos e licenças: THIRD_PARTY_NOTICES.md e arquivos de licença dos recursos.','caption')

pdf=OUT/'Documentacao_MaskyIsland.pdf'
doc=SimpleDocTemplate(str(pdf),pagesize=A4,rightMargin=54,leftMargin=54,topMargin=58,bottomMargin=55,title='Masky Island - Documentação do Jogo',author='João Henrique Nazar Tavares e Rafael Mele Porto')
doc.build(flow,onFirstPage=chrome,onLaterPages=chrome)
render=ROOT/'tmp/pdfs/entrega';render.mkdir(parents=True,exist_ok=True)
document=pdfium.PdfDocument(str(pdf))
for n in range(len(document)):
 document[n].render(scale=1.35).to_pil().save(render/f'page-{n+1:02d}.png')
print(f'PDF: {pdf}; {len(document)} páginas')
for name in ['APRESENTACAO_5_MINUTOS.md','CHECKLIST.md','PAGINA_ITCH.txt']:
 shutil.copy2(ROOT/'Documentation/Entrega'/name,OUT/name)
for relative, name in [('Documentation/IslandMenu/island-menu.png','01-menu.png'),('Documentation/CraftingGameplay/crafting-workbench.png','02-crafting.png'),('Documentation/LighthouseExterior/lighthouse-exterior.png','03-farol.png'),('Documentation/LighthouseQte/lighthouse-qte-playing.png','04-mecanismo.png')]:
 shutil.copy2(ROOT/relative,PUB/name)
with PILImage.open(ROOT/'Assets/Art/Backgrounds/Menu/IslandBeforeStorm-v1.png') as im:
 ImageOps.fit(im.convert('RGB'),(630,500)).save(PUB/'capa-itch.png')
shutil.copy2(ROOT/'THIRD_PARTY_NOTICES.md',OUT/'Windows'/'THIRD_PARTY_NOTICES.md') if (OUT/'Windows').exists() else None
