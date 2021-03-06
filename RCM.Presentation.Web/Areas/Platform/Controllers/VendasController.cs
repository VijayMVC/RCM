﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCM.Application.ApplicationInterfaces;
using RCM.Application.ViewModels;
using RCM.Application.ViewModels.VendaViewModels;
using RCM.Domain.Core.Extensions;
using RCM.Domain.DomainNotificationHandlers;
using RCM.Domain.Models.VendaModels;
using RCM.Presentation.Web.Controllers;
using RCM.Presentation.Web.Extensions;
using RCM.Presentation.Web.Filters;
using RCM.Presentation.Web.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RCM.Presentation.Web.Areas.Platform.Controllers
{
    [Authorize]
    [Area("Platform")]
    public class VendasController : BaseController
    {
        private readonly IVendaApplicationService _vendaApplicationService;
        private readonly IClienteApplicationService _clienteApplicationService;
        private readonly IProdutoApplicationService _produtoApplicationService;
        private readonly IEmpresaApplicationService _empresaApplicationService;

        public VendasController(IVendaApplicationService vendaApplicationService, IClienteApplicationService clienteApplicationService, IProdutoApplicationService produtoApplicationService, IEmpresaApplicationService empresaApplicationService, IDomainNotificationHandler domainNotificationHandler) : base(domainNotificationHandler)
        {
            _vendaApplicationService = vendaApplicationService;
            _clienteApplicationService = clienteApplicationService;
            _produtoApplicationService = produtoApplicationService;
            _empresaApplicationService = empresaApplicationService;
        }

        public IActionResult Index(Guid? clienteId, string minValor, string maxValor, string status, string dataInicial, string dataFinal, int pageNumber = 1, int pageSize = 20)
        {
            var clienteIdSpecification = new VendaClienteIdSpecification(clienteId);
            var valorSpecification = new VendaValorTotalSpecification(minValor.ToDecimal(), maxValor.ToDecimal());
            var statusSpecification = new VendaStatusSpecification(status);
            var dataSpecification = new VendaDataSpecification(dataInicial.ToDate(), dataFinal.ToDate());
            
            var list = _vendaApplicationService.Get(clienteIdSpecification
                .And(valorSpecification)
                .And(statusSpecification)
                .And(dataSpecification)
                .ToExpression());

            var viewModel = new VendasIndexViewModel
            {
                Vendas = list.ToPagedList(pageNumber, pageSize),
                Clientes = _clienteApplicationService.Get().OrderBy(c => c.Nome),
                ClienteId = clienteId,
                MinValor = minValor,
                MaxValor = maxValor,
                DataInicial = dataInicial,
                DataFinal = dataFinal,
                Status = status
            };

            return View(viewModel);
        }

        public IActionResult Details(Guid id)
        {
            var venda = _vendaApplicationService.GetById(id);
            if (venda == null)
                return NotFound();

            return View(venda);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendaViewModel venda)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return View(venda);
            }

            var commandResult = await _vendaApplicationService.Add(venda);

            if (commandResult.Success)
            {
                NotifyCommandResultSuccess();
                return RedirectToAction(nameof(Index));
            }
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return View(venda);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public IActionResult Edit(Guid id)
        {
            var venda = _vendaApplicationService.GetById(id);
            if (venda == null || venda.Status == VendaStatusEnum.Fechada)
                return NotFound();

            return View(venda);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, VendaViewModel venda)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return View(venda);
            }

            var commandResult = await _vendaApplicationService.Update(venda);

            if (commandResult.Success)
            {
                NotifyCommandResultSuccess();
                return RedirectToAction(nameof(Details), new { id = venda.Id });
            }
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return View(venda);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public IActionResult Delete(Guid id)
        {
            var venda = _vendaApplicationService.GetById(id);
            if (venda == null)
                return NotFound();

            return View(venda);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, VendaViewModel venda)
        {
            var commandResult = await _vendaApplicationService.Remove(venda);

            if (commandResult.Success)
            {
                NotifyCommandResultSuccess();
                return RedirectToAction(nameof(Index));
            }
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return View(venda);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public IActionResult AttachProduto(Guid vendaId)
        {
            VendaProdutoViewModel vendaProduto = new VendaProdutoViewModel()
            {
                VendaId = vendaId
            };

            return View(vendaProduto);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttachProduto(VendaProdutoViewModel vendaProduto)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return View(vendaProduto);
            }

            var commandResult = await _vendaApplicationService.AttachProduto(vendaProduto);

            if (commandResult.Success)
            {
                NotifyCommandResultSuccess();
                return RedirectToAction(nameof(Details), new { id = vendaProduto.VendaId });
            }
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return View(vendaProduto);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public async Task<IActionResult> RemoveProduto(Guid vendaId, Guid produtoId)
        {
            var commandResult = await _vendaApplicationService.RemoveProduto(vendaId, produtoId);

            if (commandResult.Success)
                NotifyCommandResultSuccess();
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return RedirectToAction(nameof(Details), new { id = vendaId });
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public IActionResult AttachServico(Guid vendaId)
        {
            ServicoViewModel viewModel = new ServicoViewModel
            {
                VendaId = vendaId
            };

            return View(viewModel);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttachServico(Guid vendaId, ServicoViewModel servico)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return View(servico);
            }

            var commandResult = await _vendaApplicationService.AttachServico(servico);

            if (commandResult.Success)
            {
                NotifyCommandResultSuccess();
                return RedirectToAction(nameof(Details), new { id = servico.VendaId });
            }
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return View(servico);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public async Task<IActionResult> RemoveServico(Guid vendaId, Guid servicoId)
        {
            var commandResult = await _vendaApplicationService.RemoveServico(vendaId, servicoId);

            if (commandResult.Success)
                NotifyCommandResultSuccess();
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return RedirectToAction(nameof(Details), new { id = vendaId });
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public IActionResult Checkout(Guid id)
        {
            var venda = _vendaApplicationService.GetById(id);
            if (venda == null)
                return NotFound();

            var parcelamento = new CondicaoPagamentoViewModel
            {
                Venda = venda,
                VendaId = venda.Id,
            };

            return View(parcelamento);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Guid vendaId, CondicaoPagamentoViewModel condicaoPagamento)
        {
            if (!ModelState.IsValid)
            {
                NotifyModelStateErrors();
                return View(condicaoPagamento);
            }

            var commandResult = await _vendaApplicationService.FinalizarVenda(vendaId, condicaoPagamento);

            if (commandResult.Success)
            {
                NotifyCommandResultSuccess();
                return RedirectToAction(nameof(Details), new { id = vendaId });
            }
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return View(condicaoPagamento);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public IActionResult PayInstallment(Guid vendaId, int parcelaId)
        {
            var venda = _vendaApplicationService.GetById(vendaId);
            if (venda == null)
                return NotFound();

            var viewModel = new ParcelaViewModel
            {
                VendaId = vendaId,
                Numero = parcelaId
            };

            return View(viewModel);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayInstallment(Guid vendaId, ParcelaViewModel parcela)
        {
            var commandResult = await _vendaApplicationService.PagarParcela(vendaId, parcela);

            if (commandResult.Success)
            {
                NotifyCommandResultSuccess();
                return RedirectToAction(nameof(Details), new { id = vendaId });
            }
            else
                NotifyCommandResultErrors(commandResult.Errors);

            return View(parcela);
        }

        [Authorize(Policy = "ActiveUser")]
        [ClaimAuthorization(ClaimName = "ActiveCompany", RedirectActionName = "Unattached", RedirectControllerName = "Empresa", RedirectAreaName = "Platform")]
        public IActionResult PrintDav(Guid id)
        {
            var venda = _vendaApplicationService.GetById(id);
            var empresa = _empresaApplicationService.Get();
            
            if (venda == null)
                return NotFound();
            if (empresa == null)
                return RedirectToAction(nameof(Index), "Empresas");

            var viewModel = new DAVViewModel
            {
                Venda = venda,
                Cliente = venda.Cliente,
                Empresa = empresa,
            };

            return View(viewModel);
        }

        public JsonResult GetClientes()
        {
            return Json(_clienteApplicationService.Get()
                .OrderBy(c => c.Nome));
        }

        public JsonResult GetProdutos()
        {
            return Json(_produtoApplicationService.Get()
                .OrderBy(c => c.Nome));
        }
    }
}