﻿using System;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MetricsManager.Controllers;
using MetricsManager.DB;
using MetricsManager.Services.DTO;
using MetricsManager.DB.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MetricsManager.Tests
{
    public class XUnit_MetricsManager
    {
        
        [Fact]
        public async Task Test_AgentsController()
        {
            int id = 1;
            var agentInfo = new AgentInfo() { Id = 1, AgentAddress = new Uri ("https://localhost:44390/"), Enabled = false }; 
            var _mapper_Repo = new Mock<IDBRepository<AgentInfo>>();
            var _mapper_Mock = new Mock<IMapper>();
            var agentController = new AgentsController(_mapper_Repo.Object, _mapper_Mock.Object);

            await agentController.RegisterAgent(agentInfo);
            await agentController.EnableAgentById(id);
            await agentController.DisableAgentById(id);
            agentController.GetRegisterServices();
        }

        [Fact]
        public void Test_CpuMetricsController()
        {
            int id = 1;
            DateTime dt1 = new();
            DateTime dt2 = new();
            var _logger_Mock = new Mock<ILogger<CpuMetrics>>();
            var _query_Mock = new Mock<IQueryManager<CpuMetrics>>();

            var cpuMetricsController = new CpuMetricsController(_logger_Mock.Object, _query_Mock.Object);
            cpuMetricsController.GetMetricsFromAgent(id, dt1, dt2);
        }

        [Fact]
        public void Test_DbRepository()
        {
            var _dbContext_Mock = new Mock<IDBRepository<AgentInfo>>();
            Assert.IsAssignableFrom<IQueryable<AgentInfo>>(_dbContext_Mock.Object.GetAll());
            _dbContext_Mock.Verify(a => a.GetAll(), Times.Once);
        }
    }
}